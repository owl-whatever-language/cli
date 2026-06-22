namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics;

public sealed class SymbolCollectionResult : IStageResultDiagnostics, IStageResultPerformance
{
	#region Properties
	public string Stage => "symbol_collection";
	public IDiagnosticBag Diagnostics { get; }
	public IPerformanceResult Performance { get; }
	public ISymbolScope Symbols { get; }
	public IReadOnlyCollection<ISymbolTarget> Targets { get; }
	#endregion

	#region Constructors
	public SymbolCollectionResult(IDiagnosticBag diagnostics, IPerformanceResult performance, ISymbolScope symbols, IReadOnlyCollection<ISymbolTarget> targets)
	{
		Diagnostics = diagnostics;
		Performance = performance;
		Symbols = symbols;
		Targets = targets;
	}
	#endregion
}

public sealed class SymbolCollector : BaseConcreteVisitor
{
	#region Nested types
	private readonly struct Scope(SymbolCollector collector) : IDisposable
	{
		public void Dispose() => collector.ExitScope();
	}
	#endregion

	#region Properties
	private DiagnosticBag Diagnostics { get; } = [];
	private SymbolScope ResultScope { get; }
	private SymbolScope CurrentScope { get; set; }
	private List<ISymbolTarget> Targets { get; } = [];
	private IFunction? TargetFunction { get; set; }
	private int ParameterIndex { get; set; }
	#endregion

	#region Constructors
	private SymbolCollector(ISymbolScope baseScope)
	{
		ResultScope = new("user_defined", baseScope);
		CurrentScope = ResultScope;
	}
	#endregion

	#region Functions
	public static SymbolCollectionResult Collect(ISymbolScope baseScope, IEnumerable<IConcreteSyntaxTree> trees)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			SymbolCollector collector = new(baseScope);

			foreach (IConcreteSyntaxTree tree in trees)
				collector.Visit(tree);

			return new(collector.Diagnostics, performance, collector.ResultScope, collector.Targets);
		}
	}
	#endregion

	#region Methods
	private Scope NewScope(string name, IConcreteSyntaxNode? node)
	{
		EnterScope(name, node);
		return new(this);
	}
	private void EnterScope(string name, IConcreteSyntaxNode? node) => CurrentScope = CurrentScope.NestScope(name, node);
	private void ExitScope()
	{
		if (CurrentScope == ResultScope)
			ThrowHelper.ThrowInvalidOperationException("Cannot exit the result scope.");

		CurrentScope = (SymbolScope?)CurrentScope.Parent!;
	}
	#endregion

	#region Node methods
	protected override bool Visit(IConcreteFunctionDeclarationStatementSyntax node)
	{
		IFunction? lastFunction = TargetFunction;
		int parameterIndex = ParameterIndex;

		string? name = node.Name.Value as string;
		Function function = new Function(name).WithSymbol(node);
		ICallable callable = CreateCallable(function, node);
		function.WithCallable(callable);
		Targets.Add(function);

		TargetFunction = function;
		ParameterIndex = -1;

		CurrentScope.AddSymbol(function);

		using (NewScope(name is null ? "function" : $"function({name})", node))
		{
			VisitChildren(node);
		}

		TargetFunction = lastFunction;
		ParameterIndex = parameterIndex;

		return false;
	}
	private ICallable CreateCallable(IFunction function, IConcreteFunctionDeclarationStatementSyntax node)
	{
		List<CallableParameter> parameters = [];

		foreach (IConcreteFunctionParameterSyntax decl in node.Parameters.Values)
		{
			string? name = decl.Name.Value as string;
			CallableParameter param = new(name);

			parameters.Add(param);
		}

		CallableReturn @return = new();

		return new Callable(function, parameters, @return);
	}
	protected override bool Visit(IConcreteRegularFunctionParameterSyntax node)
	{
		ParameterIndex++;

		string? name = node.Name.Value as string;
		FunctionParameter parameter = new FunctionParameter(name).WithSymbol(node);
		Targets.Add(parameter);

		CurrentScope.AddSymbol(parameter);

		if (TargetFunction?.Callable is not null)
			TargetFunction.Callable.Parameters[ParameterIndex].Parameter = parameter;

		return true;
	}
	protected override bool Visit(IConcreteVariableDeclarationStatementSyntax node)
	{
		string? name = node.Name.Value as string;
		LocalVariable variable = new LocalVariable(name).WithSymbol(node);
		Targets.Add(variable);

		CurrentScope.AddSymbol(variable);

		return true;
	}
	#endregion
}
