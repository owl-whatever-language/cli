namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics;

public sealed class SymbolResolutionResult : IStageResultDiagnostics, IStageResultPerformance, ISourceStageResult
{
	#region Properties
	public string Stage => "symbol_resolution";
	public IDiagnosticBag Diagnostics { get; }
	public IPerformanceResult Performance { get; }
	public ISymbolicSyntaxTree Tree { get; }
	public ISourceFile Source => Tree.Source;
	#endregion

	#region Constructors
	public SymbolResolutionResult(
		IDiagnosticBag diagnostics,
		IPerformanceResult performance,
		ISymbolicSyntaxTree tree)
	{
		Diagnostics = diagnostics;
		Performance = performance;
		Tree = tree;
	}
	#endregion
}

public sealed class ParallelSymbolResolutionResult : IParallelStageResult<SymbolResolutionResult>
{
	#region Properties
	public string Stage => "parallel_symbol_resolution";
	public IPerformanceResult Performance { get; }
	public IReadOnlyCollection<SymbolResolutionResult> Children { get; }
	public IEnumerable<ISymbolicSyntaxTree> Trees => Children.Select(r => r.Tree);
	#endregion

	#region Constructors
	public ParallelSymbolResolutionResult(IPerformanceResult performance, IReadOnlyCollection<SymbolResolutionResult> children)
	{
		Performance = performance;
		Children = children;
	}
	#endregion
}

public sealed class SymbolResolver : BaseConcreteToSymbolicTreeConverter, IDiagnosticProvider
{
	#region Nested types
	private readonly struct Scope(SymbolResolver resolver) : IDisposable
	{
		#region Methods
		public void Dispose() => resolver.ExitScope();
		#endregion
	}
	#endregion

	#region Properties
	public string Name => "symbol_resolver";
	private DiagnosticBag Diagnostics { get; } = [];
	private ISymbolScope BaseScope { get; }
	private ISymbolScope CurrentScope { get; set; }

	[NotNull]
	private ISourceFile? Source
	{
		get => field ?? ThrowHelper.ThrowInvalidOperationException<ISourceFile>("The source file hasn't been assigned yet.");
		set;
	}
	#endregion

	#region Constructors
	private SymbolResolver(ISymbolScope baseScope)
	{
		BaseScope = baseScope;
		CurrentScope = baseScope;
	}
	#endregion

	#region Functions
	public static SymbolResolutionResult Resolve(ISymbolScope baseScope, IConcreteSyntaxTree concrete)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			SymbolResolver resolver = new(baseScope);
			ISymbolicSyntaxTree symbolic = resolver.Convert(concrete);

			return new(resolver.Diagnostics, performance, symbolic);
		}
	}
	public static ParallelSymbolResolutionResult Resolve(ISymbolScope baseScope, IReadOnlyCollection<IConcreteSyntaxTree> trees)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			if (trees.Count is 0)
				return new(performance, []);

			if (trees.Count is 1)
			{
				SymbolResolutionResult result = Resolve(baseScope, trees.Single());
				return new(performance, [result]);
			}

			SymbolResolutionResult[] results = new SymbolResolutionResult[trees.Count];

			ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };
			Parallel.ForEach(trees, options, (tree, _, index) => results[index] = Resolve(baseScope, tree));

			return new(performance, results);
		}
	}
	#endregion

	#region Methods
	protected override SymbolicSyntaxTree Convert(IConcreteSyntaxTree tree)
	{
		Source = tree.Source;
		return base.Convert(tree);
	}
	#endregion

	#region Refined declaration methods
	protected override SymbolicVariableDeclarationStatementSyntax Convert(IConcreteVariableDeclarationStatementSyntax concrete)
	{
		Get(concrete, out IDeclaredLocalVariable variable);

		var type = Convert(concrete.Type);
		var name = Convert(concrete.Name);
		var assignment = Convert(concrete.Assignment);
		var value = Convert(concrete.Value);
		var terminator = Convert(concrete.Terminator);

		variable.Type = type.TypeInfo;

		SymbolicVariableDeclarationStatementSyntax symbolic = new(type, name, assignment, value, terminator, variable);
		Update(variable, symbolic);

		return symbolic;
	}
	protected override SymbolicFunctionDeclarationStatementSyntax Convert(IConcreteFunctionDeclarationStatementSyntax concrete)
	{
		Get(concrete, out IDeclaredFunction function);
		using (EnterScope(concrete, out ISymbolScope scope))
		{
			var name = Convert(concrete.Name);
			var start = Convert(concrete.Start);
			var parameters = Convert(concrete.Parameters);
			var end = Convert(concrete.End);
			var @return = Convert(concrete.Return);
			var body = Convert(concrete.Body);

			if (@return is ISemanticRegularFunctionReturnSyntax regularReturn)
				function.Return.Type = regularReturn.ReturnType.TypeInfo;

			SymbolicFunctionDeclarationStatementSyntax symbolic = new(name, start, parameters, end, @return, body, function, scope);
			Update(function, symbolic);

			return symbolic;
		}
	}
	protected override SymbolicRegularFunctionParameterSyntax Convert(IConcreteRegularFunctionParameterSyntax concrete)
	{
		Get(concrete, out IDeclaredFunctionParameter parameter);

		var type = Convert(concrete.Type);
		var name = Convert(concrete.Name);

		parameter.Type = type.TypeInfo;

		SymbolicRegularFunctionParameterSyntax symbolic = new(type, name, parameter);
		Update(parameter, symbolic);

		return symbolic;
	}
	#endregion

	#region Get symbol group methods
	protected override SymbolicGetExpressionSyntax Convert(IConcreteGetExpressionSyntax concrete)
	{
		ISymbolGroup symbols = GetAll(concrete.Name);
		var name = Convert(concrete.Name);

		return new(name, symbols);
	}
	protected override SymbolicRegularTypeSyntax Convert(IConcreteRegularTypeSyntax concrete)
	{
		ISymbolGroup symbols = GetAll(concrete.Name);
		var name = Convert(concrete.Name);

		IType[] typed = symbols.OfType<IType>().ToArray();

		if (typed.Length > 1)
			AddError("type_ambiguity", concrete.Name.Position, $"There are several types named '{concrete.Name.Value}', and they cannot be disambiguated.");

		return new(name, symbols, typed.SingleOrDefault() ?? SpecialTypes.Error);
	}
	protected override SymbolicEmptyTypeSyntax Convert(IConcreteEmptyTypeSyntax concrete) => new(SpecialTypes.Error);
	protected override SymbolicNestedTypeSyntax Convert(IConcreteNestedTypeSyntax concrete) => throw new NotImplementedException();
	protected override SymbolicGenericTypeSyntax Convert(IConcreteGenericTypeSyntax concrete) => throw new NotImplementedException();
	#endregion

	#region Scope helpers
	private Scope EnterScope(IConcreteSyntaxNode declaration, out ISymbolScope scope)
	{
		scope = CurrentScope.GetChild(declaration);
		CurrentScope = scope;
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
	private void Get<T>(IConcreteSyntaxNode declaration, out T symbol) where T : notnull, IDeclaredSymbol
	{
		symbol = CurrentScope.Get<T>(declaration);
	}
	private void Update(IDeclaredSymbol symbol, ISymbolicSyntaxNode declaration)
	{
		CurrentScope.Update(symbol, declaration);
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
