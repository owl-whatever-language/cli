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
	#region Nested types
	private readonly struct Scope(DeclarationFinder finder) : IDisposable
	{
		#region Methods
		public void Dispose() => finder.ExitScope();
		#endregion
	}
	#endregion

	#region Properties
	public string Name => "declaration_finder";
	private DiagnosticBag Diagnostics { get; } = [];
	private SymbolScope ResultScope { get; }
	private Stack<SymbolScope> Scopes { get; } = [];
	private SymbolScope CurrentScope { get; set; }

	private ThreadLocal<ISourceFile?> Source { get; } = new();
	#endregion

	#region Constructors
	private DeclarationFinder(ISymbolScope baseScope)
	{
		ResultScope = new("user_defined", baseScope);
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
	public override void Visit(IConcreteSyntaxTree tree)
	{
		Source.Value = tree.Source;
		base.Visit(tree);
	}
	protected override bool Visit(IConcreteVariableDeclarationStatementSyntax node)
	{
		DeclaredLocalVariable variable = new(node);
		AddSingle(variable, node.Name.Position);

		return true;
	}
	protected override bool Visit(IConcreteFunctionDeclarationStatementSyntax node)
	{
		DeclaredFunction function = new(node);
		using (NewScopeSingle("function", function, node.Signature.Name.Position))
		{
			foreach (IDeclaredFunctionParameter parameter in function.Parameters)
				Add(parameter);

			Dispatch(node.Body);
		}

		return false;
	}
	#endregion

	#region Scope methods
	private void AddSingle(IDeclaredSymbol symbol, IndexedPositionRange position)
	{
		if (symbol.Name is not null && CurrentScope.TryGetLocal(symbol.Name, out ISymbolGroup? symbols))
		{
			if (symbol is IFunction && symbols.OfType<IFunction>().Any())
				AddError("duplicate_symbol", position, $"A symbol named '{symbol.Name}' already exists in this scope. Function overloading is not yet supported.");
			else
				AddError("duplicate_symbol", position, $"A symbol named '{symbol.Name}' already exists in this scope.");
		}

		Add(symbol);
	}
	private void Add(IDeclaredSymbol symbol) => CurrentScope.Add(symbol);
	private Scope NewScopeSingle(string kind, IDeclaredSymbol symbol, IndexedPositionRange position)
	{
		AddSingle(symbol, position);

		string name = $"{kind}({symbol.Name})";
		return NewScope(name, symbol.Declaration);
	}
	private Scope NewScope(string kind, IDeclaredSymbol symbol)
	{
		Add(symbol);

		string name = $"{kind}({symbol.Name})";
		return NewScope(name, symbol.Declaration);
	}
	private Scope NewScope(string name, ISyntaxNode declaration)
	{
		SymbolScope newScope = new(name, CurrentScope);
		CurrentScope.Add(declaration, newScope);

		Scopes.Push(CurrentScope);
		CurrentScope = newScope;

		return new(this);
	}
	private void ExitScope()
	{
		if (Scopes.TryPop(out SymbolScope? scope))
			CurrentScope = scope;
		else
			ThrowHelper.ThrowInvalidOperationException($"Exiting the '{ResultScope.Name}' scope is not allowed.");
	}
	#endregion

	#region Diagnostic helpers
	private void AddError(string id, IndexedPositionRange position, string message, StackTrace? stackTrace = null)
	{
		AddDiagnostic(DiagnosticKind.Error, id, position, message, stackTrace);
	}
	private void AddDiagnostic(DiagnosticKind kind, string id, IndexedPositionRange position, string message, StackTrace? stackTrace = null)
	{
		Debug.Assert(Source.Value is not null);
		Diagnostics.Add(this, kind, id, Source.Value, position, message, stackTrace);
	}
	#endregion
}
