using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions;

namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics;

public sealed class SymbolCollectionResult : IStageResultDiagnostics, IStageResultPerformance
{
	#region Properties
	public string Stage => "symbol_collection";
	public IDiagnosticBag Diagnostics { get; }
	public IPerformanceResult Performance { get; }
	public ISymbolScope Symbols { get; }
	#endregion

	#region Constructors
	public SymbolCollectionResult(IDiagnosticBag diagnostics, IPerformanceResult performance, ISymbolScope symbols)
	{
		Diagnostics = diagnostics;
		Performance = performance;
		Symbols = symbols;
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
	#endregion

	#region Constructors
	public SymbolCollector(ISymbolScope baseScope)
	{
		ResultScope = new("user_defined", baseScope);
		CurrentScope = ResultScope;
	}
	#endregion

	#region Functions
	public static SymbolCollectionResult Collect(ISymbolScope baseScope, IEnumerable<IConcreteSyntaxTree> trees)
	{
		using PerformanceScope _ = PerformanceResult.Scope(out IPerformanceResult performance);

		SymbolCollector collector = new(baseScope);

		foreach (IConcreteSyntaxTree tree in trees)
			collector.Visit(tree);

		return new(collector.Diagnostics, performance, collector.ResultScope);
	}
	#endregion

	#region Methods
	private Scope NewScope(string name)
	{
		EnterScope(name);
		return new(this);
	}
	private void EnterScope(string name) => CurrentScope = CurrentScope.NestScope(name);
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
		string? name = node.Name.Value as string;

		Function function = new Function(name).WithSymbol(node);
		CurrentScope.AddSymbol(function);

		using (NewScope(name is null ? "function" : $"function({name})"))
		{
			VisitChildren(node);
		}

		return false;
	}
	protected override bool Visit(IConcreteRegularFunctionParameterSyntax node)
	{
		string? name = node.Name.Value as string;
		FunctionParameter parameter = new FunctionParameter(name).WithSymbol(node);

		CurrentScope.AddSymbol(parameter);

		return true;
	}
	protected override bool Visit(IConcreteVariableDeclarationStatementSyntax node)
	{
		string? name = node.Name.Value as string;
		LocalVariable variable = new LocalVariable(name).WithSymbol(node);

		CurrentScope.AddSymbol(variable);

		return true;
	}
	#endregion
}
