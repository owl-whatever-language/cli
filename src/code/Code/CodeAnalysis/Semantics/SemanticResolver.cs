using System.Text;

namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics;

public sealed class SemanticResolutionResult : ISourceStageResult, IStageResultDiagnostics, IStageResultPerformance
{
	#region Properties
	public string Stage => "semantic_resolution";
	public IDiagnosticBag Diagnostics { get; }
	public IPerformanceResult Performance { get; }
	public ISemanticSyntaxTree Tree { get; }
	public ISourceFile Source => Tree.Source;
	#endregion

	#region Constructors
	public SemanticResolutionResult(IDiagnosticBag diagnostics, IPerformanceResult performance, ISemanticSyntaxTree tree)
	{
		Diagnostics = diagnostics;
		Performance = performance;
		Tree = tree;
	}
	#endregion
}

public sealed class ParallelSemanticResolutionResult : IParallelStageResult<SemanticResolutionResult>
{
	#region Properties
	public string Stage => "semantic_resolution";
	public IPerformanceResult Performance { get; }
	public IReadOnlyCollection<SemanticResolutionResult> Children { get; }
	public IEnumerable<ISemanticSyntaxTree> Trees => Children.Select(r => r.Tree);
	#endregion

	#region Constructors
	public ParallelSemanticResolutionResult(IPerformanceResult performance, IReadOnlyCollection<SemanticResolutionResult> children)
	{
		Performance = performance;
		Children = children;
	}
	#endregion
}

public sealed class SemanticResolver : BaseDeclaredToSemanticTreeConverter, IDiagnosticProvider
{
	#region Nested types
	private readonly struct Scope(SemanticResolver resolver) : IDisposable
	{
		#region Methods
		public void Dispose() => resolver.ExitScope();
		#endregion
	}
	private readonly ref struct ValueScope<T>(ref T field, T oldValue) : IDisposable
	{
		#region Fields
		private readonly ref T _field = ref field;
		#endregion

		#region Methods
		public void Dispose() => _field = oldValue;
		#endregion
	}
	#endregion

	#region Fields
	private readonly HashSet<string> _knownDuplicateUses = [];
	private ICallableType? _currentCallable;
	private IDeclaredFunction? _currentFunction;
	#endregion

	#region Properties
	public string Name => "semantic_resolver";
	private DiagnosticBag Diagnostics { get; } = [];
	private ISourceFile Source { get; }
	private ICoreSymbolScope CoreScope { get; }
	private ISymbolScope BaseScope { get; }
	private ISymbolScope CurrentScope { get; set; }
	#endregion

	#region Constructors
	private SemanticResolver(ISourceFile source, ISymbolScope baseScope)
	{
		Source = source;
		CoreScope = baseScope.Core;
		BaseScope = baseScope;
		CurrentScope = baseScope;
	}
	#endregion

	#region Functions
	public static SemanticResolutionResult Resolve(ISymbolScope baseScope, IDeclaredSyntaxTree tree)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			SemanticResolver resolver = new(tree.Source, baseScope);
			var result = resolver.Convert(tree);

			return new(resolver.Diagnostics, performance, result);
		}
	}
	public static ParallelSemanticResolutionResult Resolve(ISymbolScope baseScope, IReadOnlyCollection<IDeclaredSyntaxTree> trees)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			if (trees.Count is 0)
				return new(performance, []);

			if (trees.Count is 1)
			{
				var tree = trees.Single();
				SemanticResolutionResult result = Resolve(baseScope, tree);
				return new(performance, [result]);
			}

			SemanticResolutionResult[] results = new SemanticResolutionResult[trees.Count];
			ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };
			Parallel.ForEach(trees, options, (tree, _, index) => results[index] = Resolve(baseScope, tree));

			return new(performance, results);
		}
	}
	#endregion

	#region Declaration methods
	protected override SemanticVariableDeclarationStatementSyntax ConvertCore(IDeclaredVariableDeclarationStatementSyntax declared)
	{
		var semantic = base.ConvertCore(declared);
		Update(semantic.Variable, semantic);

		IType valueType = semantic.Value.ResultType;
		IType variableType = semantic.Variable.Type;

		if (ShouldReportIncompatibleType(valueType, variableType))
			ReportIncompatibleType(declared.Assignment, $"A value of the type '{valueType}' cannot be assigned to a variable of the type '{variableType}'.");

		return semantic;
	}
	protected override ISemanticFunctionParameterSyntax ConvertCore(IDeclaredFunctionParameterSyntax declared)
	{
		var semantic = base.ConvertCore(declared);
		Update(semantic.Parameter, semantic);

		return semantic;
	}
	protected override SemanticFunctionDeclarationStatementSyntax ConvertCore(IDeclaredFunctionDeclarationStatementSyntax declared)
	{
		SemanticFunctionDeclarationStatementSyntax semantic;
		using (WithValue(ref _currentFunction, declared.Function))
		using (EnterScope(declared))
		{
			semantic = base.ConvertCore(declared);
			Update(semantic.Function, semantic);
		}
		Update(declared, semantic);

		return semantic;
	}
	#endregion

	#region Statements
	protected override SemanticReturnStatementSyntax ConvertCore(IDeclaredReturnStatementSyntax declared)
	{
		var semantic = base.ConvertCore(declared);
		if (_currentFunction is null)
		{
			ReportReturnNotInFunction(declared.Keyword);
			return semantic;
		}

		if (_currentFunction.Return.Type != SpecialTypes.Void && _currentFunction.Return.Type.IsNotError)
		{
			ReportIncompatibleType(semantic.Keyword, $"The function '{(_currentFunction.Name, ClassificationKind.Function)}' specifies a return type, so a return value was expected.")
				.Add(_currentFunction.Declaration.Signature.Return, lines => lines.AddLine("This is where the function specifies the return type."));

		}

		return semantic;
	}
	protected override SemanticValueReturnStatementSyntax ConvertCore(IDeclaredValueReturnStatementSyntax declared)
	{
		var semantic = base.ConvertCore(declared);
		if (_currentFunction is null)
		{
			ReportReturnNotInFunction(declared.Keyword);
			return semantic;
		}

		IType valueType = semantic.Value.ResultType;
		IType targetType = _currentFunction.Return.Type;

		if (ShouldReportIncompatibleType(valueType, targetType))
		{
			ReportIncompatibleType(declared.Value, $"A return value of the type '", valueType, "' cannot be assigned to the function's return type '", targetType, "'.")
				.Add(_currentFunction.Declaration.Signature.Return, lines =>
				{
					lines
						.AddLine("If you want to return a value, specify the return type here like so:")
						.AddLine($"{(_currentFunction.Name, ClassificationKind.Function)}(): ", targetType);
				});
		}

		return semantic;
	}

	protected override SemanticIfStatementSyntax ConvertCore(IDeclaredIfStatementSyntax declared)
	{
		var semantic = base.ConvertCore(declared);

		if (CoreScope.Bool is null)
			ReportCoreTypeNotFound(declared.Keyword, "bool", "if statements");

		CheckConditionType(semantic.Condition);

		return semantic;
	}
	protected override SemanticIfElseStatementSyntax ConvertCore(IDeclaredIfElseStatementSyntax declared)
	{
		var semantic = base.ConvertCore(declared);

		if (CoreScope.Bool is null)
			ReportCoreTypeNotFound(declared.Keyword, "bool", "if statements");

		CheckConditionType(semantic.Condition);

		return semantic;
	}
	protected override SemanticWhileStatementSyntax ConvertCore(IDeclaredWhileStatementSyntax declared)
	{
		var semantic = base.ConvertCore(declared);

		if (CoreScope.Bool is null)
			ReportCoreTypeNotFound(declared.Keyword, "bool", "while statements");

		CheckConditionType(semantic.Condition);

		return semantic;
	}
	#endregion

	#region Get expression methods
	protected override SemanticGetExpressionSyntax ConvertCore(IDeclaredGetExpressionSyntax declared)
	{
		// Note(Nightowl): This can later be used to contextually pick more appropriate symbols based on how they'll be used;

		return declared.Parent switch
		{
			_ => ConvertGeneral(declared)
		};
	}
	private SemanticGetExpressionSyntax ConvertGeneral(IDeclaredGetExpressionSyntax declared)
	{
		ISymbol? symbol = GetSingle(declared.Name, out ISymbol[] ambiguity);
		ClassificationKind? classification = symbol?.Classification;

		if (symbol is null && ambiguity.Any())
		{
			Debug.Assert(ambiguity.Length >= 2);
			ClassificationKind? shared = ambiguity.GetSharedClassification();
			classification ??= shared;

			if (classification is not null)
			{
				// Note(Nightowl):
				// If they are both of the same type, and the same name, and the same scope, then it's probably just an accident,
				// so in order to try and decrease the amount of errors, we just use the first one.
				// The same logic check will be used to suppress the error.
				symbol = ambiguity[0];
			}
		}

		var name = Convert(declared.Name, classification, symbol);
		IType resultType = GetResultType(symbol, declared.Name);

		return new(name, symbol ?? SpecialSymbols.NotFound, resultType);
	}
	#endregion

	#region Expression methods
	protected override SemanticEmptyExpressionSyntax ConvertCore(IDeclaredEmptyExpressionSyntax declared) => new(SpecialTypes.Error);
	protected override SemanticBinaryExpressionSyntax ConvertCore(IDeclaredBinaryExpressionSyntax declared)
	{
		var left = Convert(declared.Left);
		var op = Convert(declared.Operator);
		var right = Convert(declared.Right);

		OperatorKind kind = op.Kind.GetOperator();
		IFunction? operation = (left.ResultType.IsError || right.ResultType.IsError) ? null :
			left.ResultType.FindOperation(left.ResultType, right.ResultType, kind) ??
			right.ResultType.FindOperation(left.ResultType, right.ResultType, kind)
		;
		TryReportUnknownOperator("binary expression", op, operation, left.ResultType, right.ResultType);

		return new(left, op, right, operation, operation?.Return.Type ?? SpecialTypes.Error);
	}
	protected override SemanticAssignmentExpressionSyntax ConvertCore(IDeclaredAssignmentExpressionSyntax declared)
	{
		var expression = Convert(declared.Expression);
		var op = Convert(declared.Operator);
		var value = Convert(declared.Value);

		ISymbol? symbol = null;
		IType? resultType = null;
		if (expression is ISemanticGetExpressionSyntax get)
		{
			if (get.Symbol is ILocalVariable variable)
			{
				symbol = variable;
				resultType = variable.Type;
			}
			else if (get.Symbol is IFunctionParameter parameter)
			{
				symbol = parameter;
				resultType = parameter.Type;
			}
			else if (get.Symbol.IsKnown)
				ReportCantAssignToSymbol(op, get.Symbol);
		}
		else if (IsLiteral(expression))
			ReportCantAssignToLiteral(op);

		return new(expression, op, value, symbol ?? SpecialSymbols.NotFound, resultType ?? SpecialTypes.Error);
	}
	protected override SemanticCompoundAssignmentExpressionSyntax ConvertCore(IDeclaredCompoundAssignmentExpressionSyntax declared)
	{
		var expression = Convert(declared.Expression);
		var op = Convert(declared.Operator);
		var value = Convert(declared.Value);

		ISymbol? symbol = null;
		if (expression is ISemanticGetExpressionSyntax get)
		{
			if (get.Symbol is ILocalVariable or IFunctionParameter)
				symbol = get.Symbol;
			else if (get.Symbol.IsKnown)
				ReportCantAssignToSymbol(op, get.Symbol);
		}
		else if (IsLiteral(expression))
			ReportCantAssignToLiteral(op);

		OperatorKind kind = op.Kind.GetOperator();
		IFunction? operation;
		if (symbol is null)
			operation = null;
		else
		{
			operation =
				expression.ResultType.FindOperation(expression.ResultType, value.ResultType, kind) ??
				value.ResultType.FindOperation(expression.ResultType, value.ResultType, kind)
			;

			TryReportUnknownOperator("compound assignment", op, operation, expression.ResultType, value.ResultType);
		}

		return new(expression, op, value, symbol ?? SpecialSymbols.NotFound, operation, operation?.Return.Type ?? SpecialTypes.Error);
	}

	protected override SemanticTernaryExpressionSyntax ConvertCore(IDeclaredTernaryExpressionSyntax declared) => throw new NotImplementedException();
	protected override SemanticGroupedExpressionSyntax ConvertCore(IDeclaredGroupedExpressionSyntax declared) => throw new NotImplementedException();
	protected override SemanticBooleanLiteralExpressionSyntax ConvertCore(IDeclaredBooleanLiteralExpressionSyntax declared) => throw new NotImplementedException();
	protected override SemanticIntegerLiteralExpressionSyntax ConvertCore(IDeclaredIntegerLiteralExpressionSyntax declared)
	{
		IType? type = CoreScope.Int;
		if (type is null)
		{

			ReportCoreTypeNotFound(declared, "int", "number literals.");
			type = SpecialTypes.Error;
		}

		var integer = Convert(declared.Integer);

		return new(integer, (long?)integer.Value, type);
	}
	protected override SemanticBaseIntegerLiteralExpressionSyntax ConvertCore(IDeclaredBaseIntegerLiteralExpressionSyntax declared)
	{
		IType? type = CoreScope.Int;
		if (type is null)
		{
			ReportCoreTypeNotFound(declared, "int", "number literals.");
			type = SpecialTypes.Error;
		}

		var @base = Convert(declared.Base);
		var integer = Convert(declared.Integer);

		return new(@base, integer, (ulong?)integer.Value, type);
	}
	protected override SemanticDecimalLiteralExpressionSyntax ConvertCore(IDeclaredDecimalLiteralExpressionSyntax declared) => throw new NotImplementedException();
	protected override SemanticInterpolatedStringExpressionSyntax ConvertCore(IDeclaredInterpolatedStringExpressionSyntax declared)
	{
		IType? type = CoreScope.Text;
		if (type is null)
		{
			ReportCoreTypeNotFound(declared, "text", "string literals.");
			type = SpecialTypes.Error;
		}

		var start = Convert(declared.Start);
		var fragments = Convert(declared.Fragments);
		var end = Convert(declared.End);

		return new(start, fragments, end, type);
	}
	protected override SemanticStringLiteralExpressionSyntax ConvertCore(IDeclaredStringLiteralExpressionSyntax declared)
	{
		IType? type = CoreScope.Text;
		if (type is null)
		{
			ReportCoreTypeNotFound(declared, "text", "string literals.");
			type = SpecialTypes.Error;
		}

		var start = Convert(declared.Start);
		var fragments = Convert(declared.Fragments);
		var end = Convert(declared.End);

		StringBuilder builder = new();
		foreach (ISemanticStringFragmentSyntax fragment in fragments)
		{
			string? value = fragment switch
			{
				ISemanticRegularStringFragmentSyntax regular => regular.Text.Value as string,
				ISemanticEscapedStringFragmentSyntax escaped => escaped.Sequence.Value as string,
				ISemanticEscapedHexStringFragmentSyntax escaped => escaped.Sequence.Value as string,

				ISemanticInterpolatedStringFragmentSyntax => ThrowHelper.ThrowInvalidOperationException<string>($"Interpolated string fragments should not be in string literals."),
				_ => ThrowHelper.ThrowInvalidOperationException<string>($"Unknown string fragment type ({fragment.GetType().Name}).")
			};

			builder.Append(value);
		}

		string text = builder.ToString();
		return new(start, fragments, end, text, type);
	}
	#endregion

	#region Function call methods
	protected override SemanticFunctionCallExpressionSyntax ConvertCore(IDeclaredFunctionCallExpressionSyntax declared)
	{
		var expression = Convert(declared.Expression);
		ICallableType? callable = expression.ResultType as ICallableType;

		if (callable is null && expression.ResultType.IsNotError)
		{
			Diagnostics
				.BuildError(this, "type_not_callable")
				.Add(declared.Start, lines => lines.AddLine("The result type of the expression '", expression.ResultType, "' is not a type that can be called."));
		}

		using (WithValue(ref _currentCallable, callable))
		{
			var start = Convert(declared.Start);
			var arguments = Convert(declared.Arguments);
			var end = Convert(declared.End);

			// Todo(Nightowl): Check argument value types;

			return new(expression, start, arguments, end, callable, callable?.Return.Type ?? SpecialTypes.Error);
		}
	}
	#endregion

	#region Helpers
	private static bool IsLiteral(IDeclaredExpressionSyntax expression) => IsLiteral(expression.NodeEnum);
	private static bool IsLiteral(SyntaxNodeEnum value)
	{
		return value is
			SyntaxNodeEnum.BaseIntegerLiteralExpression or
			SyntaxNodeEnum.IntegerLiteralExpression or
			SyntaxNodeEnum.DecimalLiteralExpression or

			SyntaxNodeEnum.StringLiteralExpression or
			SyntaxNodeEnum.InterpolatedStringExpression or

			SyntaxNodeEnum.BooleanLiteralExpression
		;
	}
	#endregion

	#region Scope helpers
	private ValueScope<T> WithValue<T>(ref T field, T value)
	{
		T old = field;
		field = value;
		return new(ref field, old);
	}
	private Scope EnterScope(IDeclaredSyntaxNode declaration)
	{
		// Note(Nightowl):
		// We could get the scope directly from the declaration in each method,
		// however this approach ensures we don't accidentally skip any scopes,
		// as that would mess up exiting the current scope. Extra line padding;

		// I just wanted to make the comment line lengths line up;

		CurrentScope = CurrentScope.GetChild(declaration);
		return new(this);
	}
	private void ExitScope()
	{
		if (CurrentScope == BaseScope)
			ThrowHelper.ThrowInvalidOperationException($"Exiting the base scope ({BaseScope.Name}) is not allowed.");

		Debug.Assert(CurrentScope.Parent is not null);
		CurrentScope = CurrentScope.Parent;
	}
	#endregion

	#region Symbol helpers
	private ISymbolGroup GetAll(ISyntaxToken token)
	{
		if (token.Value is not string name) // Note(Nightowl): Invalid names will have already been reported during parsing;
			return new SymbolGroup();

		ISymbolGroup group = CurrentScope.GetAll(name);
		if (group.Count is 0)
		{// Note(Nightowl): Could maybe do name similarity checking here to make suggestions as extra lines;

			Diagnostics
				.BuildError(this, "symbol_not_found")
				.Add(token, lines => lines.AddLine($"No accessible symbol named '{name}' could be found."));
		}

		return group;
	}
	private ISymbol? GetSingle(ISyntaxToken token) => GetSingle<ISymbol>(token, "symbol", "symbols");
	private ISymbol? GetSingle(ISyntaxToken token, out ISymbol[] ambiguity) => GetSingle(token, "symbol", "symbols", out ambiguity);
	private T? GetSingle<T>(ISyntaxToken token, string kind, string kindPlural) where T : notnull, ISymbol
	{
		return GetSingle<T>(token, kind, kindPlural, out _);
	}
	private T? GetSingle<T>(ISyntaxToken token, string kind, string kindPlural, out T[] ambiguity)
		where T : notnull, ISymbol
	{
		if (token.Value is not string name) // Note(Nightowl): Invalid names will have already been reported during parsing;
		{
			ambiguity = [];
			return default;
		}

		if (CurrentScope.TryGet(name, out ISymbolGroup? symbols) is false)
			symbols = GetAll(token);

		if (symbols.Count is 0)
		{
			ambiguity = [];
			return default;
		}

		ambiguity = symbols.OfType<T>().ToArray();
		if (ambiguity.Length is 0)
		{
			Diagnostics
				.BuildError(this, $"{kind}_not_found")
				.Add(token, lines =>
				{
					lines.AddLine($"No accessible {kind} named '{name}' could be found.");
					if (symbols.Count is 1)
						lines.AddLine($"But a symbol with the same name was found.");
					else if (symbols.Count > 1)
						lines.AddLine("But several symbols with the same name were found.");
				});

			return default;
		}

		if (ambiguity.Length > 1)
		{
			// Note(Nightowl): Could maybe list the ambiguous symbols as extra lines here;

			Diagnostics
				.BuildError(this, $"{kind}_ambiguity")
				.Add(token, lines => lines.AddLine($"Multiple {kindPlural} named '{name}' were found, but they couldn't be disambiguated."));

			return default;
		}

		return ambiguity[0];
	}
	private void Update(IDeclaredSymbol symbol, ISemanticSyntaxNode declaration)
	{
		CurrentScope.Update(symbol, declaration);
	}
	private void Update(IDeclaredSyntaxNode oldDeclaration, ISemanticSyntaxNode newDeclaration)
	{
		CurrentScope.UpdateChild(oldDeclaration, newDeclaration);
	}
	private IType GetResultType(ISymbol? symbol, ISyntaxNode node)
	{
		if (symbol is IType)
		{
			Diagnostics
				.BuildError(this, "invalid_type_use")
				.Add(node, lines => lines.AddLine("Accessing types in this way is not yet supported."));

			return SpecialTypes.Error;
		}

		return symbol switch
		{
			ILocalVariable variable => variable.Type,
			IFunctionParameter parameter => parameter.Type,
			IFunction function => function.AsCallable,

			null => SpecialTypes.Error,
			_ => ThrowHelper.ThrowInvalidOperationException<IType>($"Unhandled symbol type ({symbol.GetType().Name}).")
		};
	}
	#endregion

	#region Helpers
	private Diagnostic ReportCoreTypeNotFound(ISyntaxNode node, string type, string missingFeature)
	{
		TextFragment typeFragment = new(type, ClassificationKind.Type);

		return Diagnostics
			.BuildError(this, "core_type_not_found")
			.Add(node, lines => lines.AddLine($"The '", typeFragment, $"' core type was not defined, as such {missingFeature} are not allowed."));
	}
	private void CheckConditionType(ISemanticExpressionSyntax condition)
	{
		if (condition.ResultType != CoreScope.Bool && condition.ResultType.IsNotError)
			ReportInvalidConditionType(condition);
	}
	private Diagnostic ReportInvalidConditionType(ISemanticExpressionSyntax condition)
	{
		TextFragment boolType = new("bool", ClassificationKind.Type);

		return Diagnostics
			.BuildError(this, "invalid_condition_type")
			.Add(condition, lines =>
			{
				lines.AddLine("Expected the condition to result in a '", boolType, "' type.");
				if (condition.ResultType.IsNotError)
					lines.AddLine("The actual result type of the condition was '", condition.ResultType, "'.");
			});
	}
	private Diagnostic ReportReturnNotInFunction(ISyntaxToken keyword)
	{
		return Diagnostics
			.BuildError(this, "return_not_in_function")
			.Add(keyword, lines => lines.AddLine("A return statement is only valid inside of a function body."));
	}

	private bool ShouldReportIncompatibleType(IType valueType, IType targetType)
	{
		if (valueType.IsError || targetType.IsError)
			return false; // Note(Nightowl): This would just result in cascade errors;

		return valueType.CanAssignTo(targetType) is false;
	}
	private Diagnostic ReportIncompatibleType(ISyntaxNode node, params IEnumerable<object?> message)
	{
		return Diagnostics
			.BuildError(this, "incompatible_type")
			.Add(node, lines => lines.AddLine(message));
	}
	private bool ShouldReportUnknownOperator(IFunction? operation, params IEnumerable<IType> types)
	{
		if (operation is not null)
			return false;

		return types.Any(t => t.IsError is false);
	}
	private void TryReportUnknownOperator(string kind, ISyntaxToken op, IFunction? operation, params IReadOnlyList<IType> types)
	{
		if (ShouldReportUnknownOperator(operation, types))
			ReportUnknownOperator(kind, op, types);
	}
	private Diagnostic ReportUnknownOperator(string kind, ISyntaxToken op, params IReadOnlyList<IType> types)
	{
		return Diagnostics
			.BuildError(this, "unknown_operator")
			.Add(op, lines =>
			{
				string typePlural = types.Count is 1 ? "type" : "types";

				List<object?> values = [$"Unknown {kind} operator for the {typePlural}: "];
				for (int i = 0; i < types.Count; i++)
				{
					if (i > 0)
					{
						if (i + 1 == types.Count)
							values.Add(" and ");
						else
							values.Add(new TextFragment(", ", ClassificationKind.Punctuation));
					}

					values.Add("'");
					values.Add(types[i]);
					values.Add("'");
				}

				lines.AddLine(values);
			});
	}
	private Diagnostic ReportCantAssignToSymbol(ISyntaxNode node, ISymbol symbol)
	{
		return Diagnostics
			.BuildError(this, "invalid_assignment")
			.Add(node, lines => lines.AddLine("Cannot assign a value to the symbol '", symbol, "'."));
	}
	private Diagnostic ReportCantAssignToLiteral(ISyntaxNode node)
	{
		// Note(Nightowl): Figure out a way to point to the literal;

		return Diagnostics
					.BuildError(this, "invalid_assignment")
					.Add(node, lines => lines.AddLine("Literals cannot be assigned to."));
	}
	#endregion
}
