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

		if (valueType.CanAssignTo(variableType) is false)
			AddError("incompatible_type", declared.Assignment.Position, $"A value of the type '{valueType}' cannot be assigned to a variable of the type '{variableType}'.");

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
			AddError("return_not_in_function", declared.Keyword.Position, "A return statement is only valid inside of a function body.");
			return semantic;
		}

		if (_currentFunction.Return.Type != SpecialTypes.Void)
			AddError("return_value_expected", semantic.Keyword.Position, $"The function '{_currentFunction.Name}' specifies a return type, so a return value was expected.");

		return semantic;
	}
	protected override SemanticValueReturnStatementSyntax ConvertCore(IDeclaredValueReturnStatementSyntax declared)
	{
		var semantic = base.ConvertCore(declared);
		if (_currentFunction is null)
		{
			AddError("return_not_in_function", declared.Keyword.Position, "A return statement is only valid inside of a function body.");
			return semantic;
		}

		IType valueType = semantic.Value.ResultType;
		IType targetType = _currentFunction.Return.Type;

		if (valueType.CanAssignTo(targetType) is false)
			AddError("incompatible_type", declared.Value.Position, $"A return value of the type '{valueType}' cannot be assigned to the function's return type '{targetType}'.");

		return semantic;
	}

	protected override SemanticIfStatementSyntax ConvertCore(IDeclaredIfStatementSyntax declared)
	{
		var semantic = base.ConvertCore(declared);

		if (semantic.Condition.ResultType != CoreScope.Bool)
			AddError("invalid_condition_type", semantic.Condition.Position, "Expected the condition to be a boolean expression.");

		return semantic;
	}
	protected override SemanticIfElseStatementSyntax ConvertCore(IDeclaredIfElseStatementSyntax declared)
	{
		var semantic = base.ConvertCore(declared);

		if (semantic.Condition.ResultType != CoreScope.Bool)
			AddError("invalid_condition_type", semantic.Condition.Position, "Expected the condition to be a boolean expression.");

		return semantic;
	}
	protected override SemanticWhileStatementSyntax ConvertCore(IDeclaredWhileStatementSyntax declared)
	{
		var semantic = base.ConvertCore(declared);

		if (semantic.Condition.ResultType != CoreScope.Bool)
			AddError("invalid_condition_type", semantic.Condition.Position, "Expected the condition to be a boolean expression.");

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
		ISymbol? symbol = GetSingle(declared.Name);
		ClassificationKind? classification = symbol switch
		{
			IFunction => ClassificationKind.Function,
			IFunctionParameter => ClassificationKind.Parameter,
			ILocalVariable => ClassificationKind.Variable,

			null => null,
			_ => ThrowHelper.ThrowInvalidOperationException<ClassificationKind?>($"Unhandled symbol type ({symbol.GetType().Name}).")
		};

		var name = Convert(declared.Name, classification);
		IType resultType = GetResultType(symbol, declared.Name.Position);

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
		IFunction? operation =
			left.ResultType.FindOperation(left.ResultType, right.ResultType, kind) ??
			right.ResultType.FindOperation(left.ResultType, right.ResultType, kind)
		;
		if (operation is null)
			AddError("unknown_operator", op.Position, $"Unknown binary expression operator '{left.ResultType} {op.Lexeme} {right.ResultType}'.");

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
			else
				AddError("invalid_assignment", op.Position, $"Cannot assign a value to the symbol '{get.Symbol.Name}'.");
		}
		else if (IsLiteral(expression))
			AddError("invalid_assignment", op.Position, "Literals cannot be assigned to.");

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
			else
				AddError("invalid_assignment", op.Position, $"Cannot assign a value to the symbol '{get.Symbol.Name}'.");
		}
		else if (IsLiteral(expression))
			AddError("invalid_assignment", op.Position, "Literals cannot be assigned to.");

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

			if (operation is null)
				AddError("unknown_operator", op.Position, $"Unknown compound assignment operator '{expression.ResultType} {op.Lexeme} {value.ResultType}'.");
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
			AddError("core_type_not_found", declared.Position, "The core 'int' type was not defined, as such number literals are not allowed.");
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
			AddError("core_type_not_found", declared.Position, "The core 'int' type was not defined, as such number literals are not allowed.");
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
			AddError("core_type_not_found", declared.Position, "The core 'text' type was not defined, as such string literals are not allowed.");
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
			AddError("core_type_not_found", declared.Position, "The core 'text' type was not defined, as such string literals are not allowed.");
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

		if (callable is null)
			AddError("type_not_callable", declared.Start.Position, "The result type of the expression is not callable.");

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
	private ISymbolGroup GetAll(ISyntaxToken token) => GetAll(token.Value as string, token.Position);
	private ISymbolGroup GetAll(string? name, IndexedPositionRange position)
	{
		if (name is null) // Note(Nightowl): Invalid names will have already been reported during parsing;
			return new SymbolGroup();

		ISymbolGroup group = CurrentScope.GetAll(name);
		if (group.Count is 0)
			AddError("symbol_not_found", position, $"No accessible symbol named '{name}' could be found.");

		return group;
	}
	private ISymbol? GetSingle(ISyntaxToken token) => GetSingle<ISymbol>(token, "symbol", "symbols");
	private T? GetSingle<T>(ISyntaxToken token, string kind, string kindPlural)
	{
		return GetSingle<T>(token.Value as string, kind, kindPlural, token.Position);
	}
	private T? GetSingle<T>(string? name, string kind, string kindPlural, IndexedPositionRange position)
	{
		if (name is null) // Note(Nightowl): Invalid names will have already been reported during parsing;
			return default;

		if (CurrentScope.TryGet(name, out ISymbolGroup? symbols) is false)
			symbols = GetAll(name, position);

		if (symbols.Count is 0)
			return default;

		T[] typed = symbols.OfType<T>().ToArray();
		if (typed.Length is 0)
		{
			AddError($"{kind}_not_found", position, $"No accessible {kind} named '{name}' could be found.");
			return default;
		}

		if (typed.Length > 1)
		{
			AddError($"{kind}_ambiguity", position, $"Multiple {kindPlural} named '{name}' were found, but they couldn't be disambiguated.");
			return default;
		}

		return typed[0];
	}
	private void Update(IDeclaredSymbol symbol, ISemanticSyntaxNode declaration)
	{
		CurrentScope.Update(symbol, declaration);
	}
	private void Update(IDeclaredSyntaxNode oldDeclaration, ISemanticSyntaxNode newDeclaration)
	{
		CurrentScope.UpdateChild(oldDeclaration, newDeclaration);
	}
	private IType GetResultType(ISymbol? symbol, IndexedPositionRange position)
	{
		if (symbol is IType)
		{
			AddError("invalid_type_use", position, "Accessing types in this way is not yet supported.");
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

	#region Diagnostic helpers
	private void AddError(string id, IndexedPositionRange position, string message, StackTrace? stackTrace = null)
	{
		AddDiagnostic(DiagnosticKind.Error, id, position, message, stackTrace);
	}
	private void AddDiagnostic(DiagnosticKind kind, string id, IndexedPositionRange position, string message, StackTrace? stackTrace = null)
	{
		Diagnostics.Add(this, kind, id, Source, position, message, stackTrace);
	}
	#endregion
}
