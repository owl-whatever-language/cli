namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics;

public sealed class DeclarationDiscoveryResult : IStageResultDiagnostics, IStageResultPerformance
{
	#region Properties
	public string Stage => "declaration_discovery";
	public IDiagnosticBag Diagnostics { get; }
	public IPerformanceResult Performance { get; }
	public ISymbolScope ResultScope { get; }
	#endregion

	#region Constructors
	public DeclarationDiscoveryResult(
		IDiagnosticBag diagnostics,
		IPerformanceResult performance,
		ISymbolScope resultScope)
	{
		Diagnostics = diagnostics;
		Performance = performance;
		ResultScope = resultScope;
	}
	#endregion
}

public sealed class DeclarationFinder : BaseConcreteVisitor, IDiagnosticProvider
{
	#region Properties
	public string Name => "declaration_finder";
	private DiagnosticBag Diagnostics { get; } = [];
	private IMutableSymbolScope ResultScope { get; }
	private Stack<IMutableSymbolScope> Scopes { get; } = [];
	private IMutableSymbolScope CurrentScope { get; set; }
	#endregion

	#region Constructors
	private DeclarationFinder(ISymbolScope baseScope)
	{
		ResultScope = new SymbolScope(baseScope, "user_defined");
		CurrentScope = ResultScope;
	}
	#endregion

	#region Functions
	public static DeclarationDiscoveryResult Discover(ISymbolScope baseScope, IReadOnlyCollection<IConcreteSyntaxTree> trees)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			DeclarationFinder finder = new(baseScope);

			if (trees.Count is 1)
				finder.Visit(trees.Single());
			else if (trees.Count > 1)
			{
				ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };
				Parallel.ForEach(trees, options, finder.Visit);
			}

			return new(finder.Diagnostics, performance, finder.ResultScope);
		}
	}
	#endregion

	#region Methods
	protected override bool Visit(IConcreteVariableDeclarationStatementSyntax node)
	{
		DeclaredLocalVariable variable = new(node);
		AddSingle(variable, node.Name);

		return true;
	}
	protected override bool Visit(IConcreteFunctionDeclarationStatementSyntax node)
	{
		DeclaredFunction function = new(node);
		using (NewScopeSingle(function, node.Signature.Name))
		{
			foreach (IDeclaredFunctionParameter parameter in function.Parameters)
				Add(parameter);

			Dispatch(node.Body);
		}

		return false;
	}
	#endregion

	#region Scope methods
	private void AddSingle(IDeclaredSymbol symbol, IConcreteToken nameToken)
	{
		if (symbol.Name is not null && CurrentScope.TrySearch(symbol.Name, includeParents: false, out ISymbolCollection? symbols))
			Diagnostics.ReportDuplicate(this, nameToken, symbol, symbols);

		Add(symbol);
	}
	private void Add(IDeclaredSymbol symbol) => CurrentScope.Add(symbol);
	private DelegateScope NewScopeSingle(IDeclaredSymbol symbol, IConcreteToken nameToken)
	{
		AddSingle(symbol, nameToken);
		return EnterNewScope(symbol);
	}
	private DelegateScope NewScope(IDeclaredSymbol symbol)
	{
		Add(symbol);

		return EnterNewScope(symbol);
	}
	private DelegateScope EnterNewScope(IDeclaredSymbol symbol)
	{
		IMutableSymbolScope newScope = CurrentScope.AddScope(symbol);

		Scopes.Push(CurrentScope);
		CurrentScope = newScope;

		return new(ExitScope);
	}
	private void ExitScope()
	{
		if (Scopes.TryPop(out IMutableSymbolScope? scope))
			CurrentScope = scope;
		else
			ThrowHelper.ThrowInvalidOperationException($"Exiting the '{ResultScope.Name}' scope is not allowed.");
	}
	#endregion
}
