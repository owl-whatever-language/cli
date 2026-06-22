using System.Text;

namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics;

public sealed class SemanticResolutionResult : ISourceStageResult, IStageResultDiagnostics, IStageResultPerformance
{
	#region Properties
	public string Stage => "semantic_resolution";
	public ISourceFile Source => Tree.Source;
	public IDiagnosticBag Diagnostics { get; }
	public IPerformanceResult Performance { get; }
	public ISemanticSyntaxTree Tree { get; }
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
	public string Stage => "parallel_semantic_resolution";
	public IPerformanceResult Performance { get; }
	public IReadOnlyCollection<SemanticResolutionResult> Children { get; }
	#endregion

	#region Constructors
	public ParallelSemanticResolutionResult(IPerformanceResult performance, IReadOnlyCollection<SemanticResolutionResult> children)
	{
		Performance = performance;
		Children = children;
	}
	#endregion
}

public sealed class SemanticResolver : BaseConcreteToSemanticTreeConverter, IDiagnosticProvider
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
	private IFunction? _targetFunction;
	private int _parameterIndex;
	#endregion

	#region Properties
	public string Name => "semantic_resolver";
	private DiagnosticBag Diagnostics { get; } = [];
	private ISourceFile Source { get; }
	private ISymbolScope BaseScope { get; }
	private ISymbolScope Symbols { get; set; }
	#endregion

	#region Constructors
	private SemanticResolver(ISourceFile source, ISymbolScope symbols)
	{
		Source = source;
		BaseScope = symbols;
		Symbols = symbols;
	}
	#endregion

	#region Functions
	public static SemanticResolutionResult Resolve(IConcreteSyntaxTree tree, ISymbolScope baseScope)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			SemanticResolver resolver = new(tree.Source, baseScope);
			SemanticSyntaxTree converted = resolver.Convert(tree);

			return new(resolver.Diagnostics, performance, converted);
		}
	}
	public static ParallelSemanticResolutionResult Resolve(IReadOnlyCollection<IConcreteSyntaxTree> trees, ISymbolScope baseScope)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			if (trees.Count is 1)
			{
				SemanticResolutionResult result = Resolve(trees.Single(), baseScope);
				return new(performance, [result]);
			}

			SemanticResolutionResult[] results = new SemanticResolutionResult[trees.Count];
			ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };

			Parallel.ForEach(trees, options, (tree, _, index) =>
			{
				SemanticResolutionResult result = Resolve(tree, baseScope);
				results[index] = result;
			});

			return new(performance, results);
		}
	}
	#endregion

	#region Methods
	private Scope EnterScope(IConcreteSyntaxNode node)
	{
		Symbols = Symbols.GetChild(node);
		return new(this);
	}
	private void ExitScope()
	{
		if (Symbols == BaseScope)
			ThrowHelper.ThrowInvalidOperationException("Cannot exit the base scope.");

		Debug.Assert(Symbols.Parent is not null);
		Symbols = Symbols.Parent;
	}
	#endregion

	#region Type methods
	protected override SemanticEmptyTypeSyntax Convert(IConcreteEmptyTypeSyntax concrete) => new(null);
	protected override SemanticRegularTypeSyntax Convert(IConcreteRegularTypeSyntax concrete)
	{
		INamedTypeInfo? type = GetTarget<INamedTypeInfo>(concrete.Name, "type");
		SemanticToken name = Convert(concrete.Name, type?.Symbol);

		return new(name, type);
	}
	protected override SemanticNestedTypeSyntax Convert(IConcreteNestedTypeSyntax concrete) => throw new NotImplementedException();
	protected override SemanticGenericTypeSyntax Convert(IConcreteGenericTypeSyntax concrete) => throw new NotImplementedException();
	#endregion

	#region Statement methods
	protected override SemanticVariableDeclarationStatementSyntax Convert(IConcreteVariableDeclarationStatementSyntax concrete)
	{
		ISemanticTypeSyntax type = Convert(concrete.Type);
		ILocalVariable variable = Symbols.GetTarget<ILocalVariable>(concrete, out IDeclaredSymbol declared);
		variable.Type = type.TypeInfo;
		variable.Lock();

		SemanticToken name = Convert(concrete.Name, variable.Symbol);
		SemanticToken assignment = Convert(concrete.Assignment);
		ISemanticExpressionSyntax value = Convert(concrete.Value);
		SemanticToken terminator = Convert(concrete.Terminator);

		SemanticVariableDeclarationStatementSyntax semantic = new(type, name, assignment, value, terminator, variable);
		declared.Declaration = semantic;

		return semantic;
	}
	protected override SemanticFunctionDeclarationStatementSyntax Convert(IConcreteFunctionDeclarationStatementSyntax concrete)
	{
		IFunction function = Symbols.GetTarget<IFunction>(concrete, out IDeclaredSymbol declared);
		using (WithValue(ref _targetFunction, function))
		using (WithValue(ref _parameterIndex, -1))
		using (EnterScope(concrete))
		{
			SemanticToken name = Convert(concrete.Name, function.Symbol);
			SemanticToken start = Convert(concrete.Start);
			SyntaxList<ISemanticFunctionParameterSyntax, ISemanticToken> parameters = Convert(concrete.Parameters);
			SemanticToken end = Convert(concrete.End);
			ISemanticFunctionReturnSyntax @return = Convert(concrete.Return);
			ISemanticFunctionBodySyntax body = Convert(concrete.Body);

			function.Lock();

			SemanticFunctionDeclarationStatementSyntax semantic = new(name, start, parameters, end, @return, body, function);
			declared.Declaration = semantic;

			return semantic;
		}
	}
	#endregion

	#region Function parameter methods
	protected override ISemanticFunctionParameterSyntax Convert(IConcreteFunctionParameterSyntax concrete)
	{
		_parameterIndex++;
		ISemanticFunctionParameterSyntax semantic = base.Convert(concrete);

		_ = Symbols.GetTarget<IFunctionParameter>(concrete, out IDeclaredSymbol declared);
		declared.Declaration = semantic;

		return semantic;
	}
	protected override SemanticRegularFunctionParameterSyntax Convert(IConcreteRegularFunctionParameterSyntax concrete)
	{
		ICallableParameter? callable = _targetFunction?.Callable?.Parameters[_parameterIndex];

		ISemanticTypeSyntax type = Convert(concrete.Type);
		IFunctionParameter parameter = Symbols.GetTarget<IFunctionParameter>(concrete, out _);

		parameter.Type = type.TypeInfo;
		parameter.Lock();

		if (callable is not null)
		{
			callable.Type = type.TypeInfo;
			//callable.Parameter = parameter;
			callable.WithSymbol(parameter.Name, concrete).Lock();
		}

		SemanticToken name = Convert(concrete.Name, parameter.Symbol);

		return new(type, name, parameter);
	}
	#endregion

	#region Function return methods
	protected override SemanticRegularFunctionReturnSyntax Convert(IConcreteRegularFunctionReturnSyntax concrete)
	{
		var node = base.Convert(concrete);

		ICallableReturn? callable = _targetFunction?.Callable?.Return;
		if (callable is not null)
		{
			callable.Type = node.ReturnType.TypeInfo;
			callable.Lock();
		}

		return node;
	}
	protected override SemanticEmptyFunctionReturnSyntax Convert(IConcreteEmptyFunctionReturnSyntax concrete)
	{
		ICallableReturn? callable = _targetFunction?.Callable?.Return;
		if (callable is not null)
		{
			callable.Type = SpecialTypes.Void;
			callable.Lock();
		}

		return base.Convert(concrete);
	}
	#endregion

	#region Function call methods
	protected override SemanticFunctionCallExpressionSyntax Convert(IConcreteFunctionCallExpressionSyntax concrete)
	{
		using (WithValue(ref _targetFunction))
		using (WithValue(ref _parameterIndex))
		{
			ISemanticExpressionSyntax expression = Convert(concrete.Expression);
			ICallable? callable = expression.ResultType as ICallable;
			_parameterIndex = -1;

			if (callable is null)
				AddError("not_callable", concrete.Start.Position, $"The value of the type '{expression.ResultType}' cannot be called.");

			SemanticToken start = Convert(concrete.Start);
			SyntaxList<ISemanticFunctionArgumentSyntax, ISemanticToken> arguments = Convert(concrete.Arguments);
			SemanticToken end = Convert(concrete.End);

			return new(expression, start, arguments, end, callable, callable?.Return.Type);
		}
	}
	protected override ISemanticFunctionArgumentSyntax Convert(IConcreteFunctionArgumentSyntax concrete)
	{
		_parameterIndex++;
		return base.Convert(concrete);
	}
	protected override SemanticRegularFunctionArgumentSyntax Convert(IConcreteRegularFunctionArgumentSyntax concrete)
	{
		ICallableParameter? parameter = _targetFunction?.Callable?.Parameters[_parameterIndex];
		ISemanticExpressionSyntax value = Convert(concrete.Value);

		return new(value, parameter);
	}
	protected override SemanticNamedFunctionArgumentSyntax Convert(IConcreteNamedFunctionArgumentSyntax concrete)
	{
		string? nameValue = concrete.Name.Value as string;
		ICallableParameter? parameter = _targetFunction?.Callable?.Parameters.FirstOrDefault(p => p.Name == nameValue);

		SemanticToken name = Convert(concrete.Name, parameter?.Symbol);
		SemanticToken separator = Convert(concrete.Separator);
		ISemanticExpressionSyntax value = Convert(concrete.Value);

		return new(name, separator, value, parameter);
	}
	#endregion

	#region Expression methods
	protected override SemanticEmptyExpressionSyntax Convert(IConcreteEmptyExpressionSyntax concrete) => new(null);
	protected override SemanticGetExpressionSyntax Convert(IConcreteGetExpressionSyntax concrete)
	{
		ISymbol? symbol;

		if (concrete.Parent is IConcreteFunctionCallExpressionSyntax)
			symbol = GetTarget<IFunction>(concrete.Name, "function")?.Symbol;
		else
			symbol = GetTarget<ISymbolTarget>(concrete.Name, "symbol")?.Symbol;

		ITypeInfo? type = GetType(symbol);
		ClassificationKind? classification = GetSymbolClassification(symbol);

		SemanticToken name = Convert(concrete.Name, classification ?? concrete.Name.Classification, symbol);
		return new(name, symbol, type);
	}
	protected override SemanticBooleanLiteralExpressionSyntax Convert(IConcreteBooleanLiteralExpressionSyntax concrete) => throw new NotImplementedException();
	protected override SemanticIntegerLiteralExpressionSyntax Convert(IConcreteIntegerLiteralExpressionSyntax concrete) => throw new NotImplementedException();
	protected override SemanticBaseIntegerLiteralExpressionSyntax Convert(IConcreteBaseIntegerLiteralExpressionSyntax concrete) => throw new NotImplementedException();
	protected override SemanticBinaryExpressionSyntax Convert(IConcreteBinaryExpressionSyntax concrete) => throw new NotImplementedException();
	protected override SemanticGroupedExpressionSyntax Convert(IConcreteGroupedExpressionSyntax concrete) => throw new NotImplementedException();
	protected override SemanticInterpolatedStringExpressionSyntax Convert(IConcreteInterpolatedStringExpressionSyntax concrete)
	{
		SemanticToken start = Convert(concrete.Start);
		ISyntaxList<ISemanticStringFragmentSyntax> fragments = Convert(concrete.Fragments);
		SemanticToken end = Convert(concrete.End);

		return new(start, fragments, end, SpecialTypes.Text);
	}
	protected override SemanticStringLiteralExpressionSyntax Convert(IConcreteStringLiteralExpressionSyntax concrete)
	{
		SemanticToken start = Convert(concrete.Start);
		ISyntaxList<ISemanticStringFragmentSyntax> fragments = Convert(concrete.Fragments);
		SemanticToken end = Convert(concrete.End);

		StringBuilder valueBuilder = new();

		foreach (ISemanticStringFragmentSyntax fragment in fragments)
		{
			string? text = fragment switch
			{
				SemanticRegularStringFragmentSyntax r => r.Text.Value as string,
				SemanticEscapedStringFragmentSyntax r => r.Sequence.Value as string,
				SemanticEscapedHexStringFragmentSyntax r => r.Sequence.Value as string,

				_ => "",
			};

			valueBuilder.Append(text);
		}

		string value = valueBuilder.ToString();

		return new(start, fragments, end, value, SpecialTypes.Text);
	}
	protected override SemanticDecimalLiteralExpressionSyntax Convert(IConcreteDecimalLiteralExpressionSyntax concrete) => throw new NotImplementedException();
	protected override SemanticTernaryExpressionSyntax Convert(IConcreteTernaryExpressionSyntax concrete) => throw new NotImplementedException();
	#endregion

	#region Helpers
	private ValueScope<T> WithValue<T>(ref T field) => new(ref field, field);
	private ValueScope<T> WithValue<T>(ref T field, T newValue)
	{
		T oldValue = field;
		field = newValue;
		return new(ref field, oldValue);
	}

	private T? GetTarget<T>(IConcreteToken nameToken, string targetKind) where T : notnull, ISymbolTarget
	{
		string? name = nameToken.Value as string;
		return GetTarget<T>(name, targetKind, nameToken.Position);
	}
	private T? GetTarget<T>(string? symbolName, string targetKind, IndexedPositionRange position) where T : notnull, ISymbolTarget
	{
		if (symbolName is null)
			return default;

		if (Symbols.TryGetSymbol(symbolName, out ISymbolGroup? group) is false)
		{
			AddError("no_symbol", position, $"Couldn't find any symbols named '{symbolName}'.");
			return default;
		}

		IReadOnlyList<T> typed = group.ForTarget<T>();
		if (typed.Count is 0)
		{
			AddError("no_symbol", position, $"Couldn't find a {targetKind} named '{symbolName}'.");
			return default;
		}

		if (typed.Count > 1)
		{
			AddError("symbol_ambiguity", position, $"Several implementations exist for a {targetKind} named '{symbolName}'.");
			return default;
		}

		return typed[0];
	}

	private void AddError(string id, IndexedPositionRange position, string message, StackTrace? stackTrace = null)
	{
		AddDiagnostic(DiagnosticKind.Error, id, position, message, stackTrace);
	}
	private void AddDiagnostic(DiagnosticKind kind, string id, IndexedPositionRange position, string message, StackTrace? stackTrace = null)
	{
		Diagnostics.Add(new Diagnostic()
		{
			Provider = this,
			Kind = kind,
			Id = id,
			StackTrace = stackTrace,

			Location = new DiagnosticSourceLocation(Source, position),
			Message = message
		});
	}

	private ITypeInfo? GetType(ISymbol? symbol)
	{
		return symbol?.Target switch
		{
			IFunction function => function.Callable,
			IFunctionParameter parameter => parameter.Type,
			ILocalVariable variable => variable.Type,

			_ => null
		};
	}

	private ClassificationKind? GetSymbolClassification(ISymbol? symbol) => GetTargetClassification(symbol?.Target);
	private ClassificationKind? GetTargetClassification(ISymbolTarget? target)
	{
		return target switch
		{
			IFunction => ClassificationKind.Function,
			ITypeInfo => ClassificationKind.Type,
			ILocalVariable => ClassificationKind.Variable,
			IFunctionParameter => ClassificationKind.Parameter,

			_ => null,
		};
	}
	#endregion
}
