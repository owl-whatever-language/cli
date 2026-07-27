namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics;

public sealed class DeclarationResolutionResult : IStageResultDiagnostics, IStageResultPerformance, ISourceStageResult
{
	#region Properties
	public string Stage => "declaration_resolution";
	public IDiagnosticBag Diagnostics { get; }
	public IPerformanceResult Performance { get; }
	public IDeclaredSyntaxTree Tree { get; }
	public ISourceFile Source => Tree.Source;
	#endregion

	#region Constructors
	public DeclarationResolutionResult(
		IDiagnosticBag diagnostics,
		IPerformanceResult performance,
		IDeclaredSyntaxTree tree)
	{
		Diagnostics = diagnostics;
		Performance = performance;
		Tree = tree;
	}
	#endregion
}

public sealed class ParallelDeclarationResolutionResult : IParallelStageResult<DeclarationResolutionResult>
{
	#region Properties
	public string Stage => "declaration_resolution";
	public IPerformanceResult Performance { get; }
	public IReadOnlyCollection<DeclarationResolutionResult> Children { get; }
	public IEnumerable<IDeclaredSyntaxTree> Trees => Children.Select(r => r.Tree);
	#endregion

	#region Constructors
	public ParallelDeclarationResolutionResult(IPerformanceResult performance, IReadOnlyCollection<DeclarationResolutionResult> children)
	{
		Performance = performance;
		Children = children;
	}
	#endregion
}

public sealed class DeclarationResolver : BaseConcreteToDeclaredTreeConverter, IDiagnosticProvider
{
	#region Fields
	private IDeclaredFunction? _currentFunction;
	#endregion

	#region Properties
	public string Name => "symbol_resolver";
	private ISourceFile Source { get; }
	private DiagnosticBag Diagnostics { get; } = [];
	private ISymbolScope BaseScope { get; }
	private ISymbolScope CurrentScope { get; set; }
	#endregion

	#region Constructors
	private DeclarationResolver(ISourceFile source, ISymbolScope baseScope)
	{
		Source = source;
		BaseScope = baseScope;
		CurrentScope = baseScope;
	}
	#endregion

	#region Functions
	public static DeclarationResolutionResult Resolve(ISymbolScope baseScope, IConcreteSyntaxTree concrete)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			DeclarationResolver resolver = new(concrete.Source, baseScope);
			IDeclaredSyntaxTree declared = resolver.Convert(concrete);

			return new(resolver.Diagnostics, performance, declared);
		}
	}
	public static ParallelDeclarationResolutionResult Resolve(ISymbolScope baseScope, IReadOnlyCollection<IConcreteSyntaxTree> trees)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			if (trees.Count is 0)
				return new(performance, []);

			if (trees.Count is 1)
			{
				DeclarationResolutionResult result = Resolve(baseScope, trees.Single());
				return new(performance, [result]);
			}

			DeclarationResolutionResult[] results = new DeclarationResolutionResult[trees.Count];

			ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };
			Parallel.ForEach(trees, options, (tree, _, index) => results[index] = Resolve(baseScope, tree));

			return new(performance, results);
		}
	}
	#endregion

	#region Refine declaration methods
	protected override DeclaredDocumentSyntax ConvertCore(IConcreteDocumentSyntax concrete)
	{
		var statements = Convert(concrete.Statements);
		var endOfInput = Convert(concrete.EndOfInput);

		return new(statements, endOfInput, CurrentScope);
	}
	protected override DeclaredVariableDeclarationStatementSyntax ConvertCore(IConcreteVariableDeclarationStatementSyntax concrete)
	{
		Get(concrete, out IDeclaredLocalVariable variable);

		var type = Convert(concrete.Type);
		var name = Convert(concrete.Name, variable);
		var assignment = Convert(concrete.Assignment);
		var value = Convert(concrete.Value);
		var terminator = Convert(concrete.Terminator);

		variable.Type = type.TypeInfo;

		DeclaredVariableDeclarationStatementSyntax declared = new(type, name, assignment, value, terminator, variable);
		variable.Declaration = declared;

		return declared;
	}
	protected override DeclaredFunctionDeclarationStatementSyntax ConvertCore(IConcreteFunctionDeclarationStatementSyntax concrete)
	{
		Get(concrete, out IDeclaredFunction function);
		using (Value.Scope(ref _currentFunction, function))
		using (EnterScope(concrete, out IMutableDeclaredSymbolScope scope))
		{
			var signature = Convert(concrete.Signature);
			var body = Convert(concrete.Body);

			function.Return.Type = signature.Return switch
			{
				IDeclaredRegularFunctionReturnSyntax regular => regular.ReturnType.TypeInfo,
				IDeclaredEmptyFunctionReturnSyntax => SpecialTypes.Void,

				_ => ThrowHelper.ThrowInvalidOperationException<IType>($"Unhandled function return type {signature.Return.GetType().Name}"),
			};

			DeclaredFunctionDeclarationStatementSyntax declared = new(signature, body, function, scope);
			function.Declaration = declared;
			scope.Declaration = declared;

			return declared;
		}
	}
	protected override DeclaredFunctionDeclarationSignatureSyntax ConvertCore(IConcreteFunctionDeclarationSignatureSyntax concrete)
	{
		IDeclaredToken? keyword = Convert(concrete.Keyword);
		IDeclaredToken name = Convert(concrete.Name, _currentFunction);
		IDeclaredToken start = Convert(concrete.Start);
		ISyntaxList<IDeclaredFunctionParameterSyntax, IDeclaredToken> parameters = Convert(concrete.Parameters);
		IDeclaredToken end = Convert(concrete.End);
		IDeclaredFunctionReturnSyntax @return = Convert(concrete.Return);

		return new(
			keyword,
			name,
			start,
			parameters,
			end,
			@return);
	}
	protected override DeclaredRegularFunctionParameterSyntax ConvertCore(IConcreteRegularFunctionParameterSyntax concrete)
	{
		Get(concrete, out IDeclaredFunctionParameter parameter);

		var type = Convert(concrete.Type);
		var name = Convert(concrete.Name, parameter);

		parameter.Type = type.TypeInfo;

		DeclaredRegularFunctionParameterSyntax declared = new(type, name, parameter);
		parameter.Declaration = declared;

		return declared;
	}
	#endregion

	#region Type methods
	protected override DeclaredRegularTypeSyntax ConvertCore(IConcreteRegularTypeSyntax concrete)
	{
		INamedType? type = GetSingle<INamedType>(concrete.Name, "type", "types");
		var name = Convert(concrete.Name, type);

		return new(
			name,
			(ISymbol?)type ?? Symbol.Unknown,
			(IType?)type ?? SpecialTypes.Error);
	}
	protected override DeclaredEmptyTypeSyntax ConvertCore(IConcreteEmptyTypeSyntax concrete) => new(SpecialTypes.Error);
	protected override DeclaredNestedTypeSyntax ConvertCore(IConcreteNestedTypeSyntax concrete) => throw new NotImplementedException();
	protected override DeclaredGenericTypeSyntax ConvertCore(IConcreteGenericTypeSyntax concrete) => throw new NotImplementedException();
	#endregion

	#region Scope helpers
	private DelegateScope EnterScope(IConcreteSyntaxNode declaration, out IMutableDeclaredSymbolScope scope)
	{
		scope = CurrentScope.GetScope(declaration);
		CurrentScope = scope;

		return new(ExitScope);
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
	private ISymbolCollection GetAll(ISyntaxToken token, string kind)
	{
		if (token.Value is not string name) // Note(Nightowl): Invalid names will have already been reported during parsing;
			return new SymbolCollection();

		ISymbolCollection group = CurrentScope.Search(name).All;
		if (group.Count is 0)
		{
			ISymbol? alternative = CurrentScope.GetAlternative(name).FirstOrDefault();
			Diagnostics.ReportNotFound(this, token, kind, 0, alternative);
		}

		return group;
	}
	private ISymbol? GetSingle(ISyntaxToken token) => GetSingle<ISymbol>(token, "symbol", "symbols");
	private T? GetSingle<T>(ISyntaxToken token, string kind, string kindPlural) where T : notnull, ISymbol
	{
		return GetSingle<T>(token, kind, kindPlural, out _);
	}
	private T? GetSingle<T>(ISyntaxToken token, string kind, string kindPlural, out T[] ambiguity) where T : notnull, ISymbol
	{
		if (token.Value is not string name) // Note(Nightowl): Invalid names will have already been reported during parsing;
		{
			ambiguity = [];
			return default;
		}

		if (CurrentScope.TrySearchFirst(name, out ISymbolCollection? symbols) is false)
			symbols = GetAll(token, kind);

		if (symbols.Count is 0)
		{
			ambiguity = [];
			return default;
		}

		ambiguity = symbols.OfType<T>().ToArray();

		if (ambiguity.Length is 0)
		{
			ISymbol? alternative = CurrentScope.GetAlternative<T>(name).FirstOrDefault();
			Diagnostics.ReportNotFound(this, token, kind, symbols.Count, alternative);

			return default;
		}

		if (ambiguity.Length > 1)
		{
			// Note(Nightowl): Could maybe list the ambiguous symbols as extra lines here;
			Diagnostics.ReportAmbiguity(this, token, kind, kindPlural);

			return default;
		}

		return ambiguity[0];
	}
	private void Get<T>(IConcreteSyntaxNode declaration, out T symbol) where T : notnull, IMutableDeclaredSymbol
	{
		symbol = CurrentScope.Get<T>(declaration);
	}
	#endregion
}
