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

public sealed class SymbolResolver : BaseConcreteToDeclaredTreeConverter, IDiagnosticProvider
{
	#region Nested types
	private readonly struct Scope(SymbolResolver resolver) : IDisposable
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
	private SymbolResolver(ISourceFile source, ISymbolScope baseScope)
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
			SymbolResolver resolver = new(concrete.Source, baseScope);
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
		Update(variable, declared);

		return declared;
	}
	protected override DeclaredFunctionDeclarationStatementSyntax ConvertCore(IConcreteFunctionDeclarationStatementSyntax concrete)
	{
		DeclaredFunctionDeclarationStatementSyntax declared;

		Get(concrete, out IDeclaredFunction function);
		using (WithValue(ref _currentFunction, function))
		using (EnterScope(concrete, out ISymbolScope scope))
		{
			var signature = Convert(concrete.Signature);
			var body = Convert(concrete.Body);

			function.Return.Type = signature.Return switch
			{
				IDeclaredRegularFunctionReturnSyntax regular => regular.ReturnType.TypeInfo,
				IDeclaredEmptyFunctionReturnSyntax => SpecialTypes.Void,

				_ => ThrowHelper.ThrowInvalidOperationException<IType>($"Unhandled function return type {signature.Return.GetType().Name}"),
			};

			declared = new(signature, body, function, scope);
			Update(function, declared);
		}

		Update(concrete, declared);

		return declared;
	}
	protected override DeclaredFunctionDeclarationSignatureSyntax ConvertCore(IConcreteFunctionDeclarationSignatureSyntax concrete)
	{
		IDeclaredToken? keyword = Convert(concrete.Keyword);
		IDeclaredToken name = ConvertCore(concrete.Name, _currentFunction);
		IDeclaredToken start = ConvertCore(concrete.Start);
		ISyntaxList<IDeclaredFunctionParameterSyntax, IDeclaredToken> parameters = ConvertCore(concrete.Parameters);
		IDeclaredToken end = ConvertCore(concrete.End);
		IDeclaredFunctionReturnSyntax @return = ConvertCore(concrete.Return);

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
		Update(parameter, declared);

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
			(ISymbol?)type ?? SpecialSymbols.NotFound,
			(IType?)type ?? SpecialTypes.Error);
	}
	protected override DeclaredEmptyTypeSyntax ConvertCore(IConcreteEmptyTypeSyntax concrete) => new(SpecialTypes.Error);
	protected override DeclaredNestedTypeSyntax ConvertCore(IConcreteNestedTypeSyntax concrete) => throw new NotImplementedException();
	protected override DeclaredGenericTypeSyntax ConvertCore(IConcreteGenericTypeSyntax concrete) => throw new NotImplementedException();
	#endregion

	#region Scope helpers
	private ValueScope<T> WithValue<T>(ref T field, T value)
	{
		T old = field;
		field = value;
		return new(ref field, old);
	}
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
	private ISymbolGroup GetAll(ISyntaxToken token)
	{
		if (token.Value is not string name) // Note(Nightowl): Invalid names will have already been reported during parsing;
			return new SymbolGroup();

		ISymbolGroup group = CurrentScope.GetAll(name);
		if (group.Count is 0)
		{
			// Note(Nightowl): Could maybe do name similarity checking here to make suggestions as extra lines;

			Diagnostics
				.BuildError(this, "symbol_not_found")
				.Add(token, lines => lines.AddLine($"No accessible symbol named '", token, "' could be found."));
		}

		return group;
	}
	private ISymbol? GetSingle(ISyntaxToken token) => GetSingle<ISymbol>(token, "symbol", "symbols");
	private T? GetSingle<T>(ISyntaxToken token, string kind, string kindPlural)
	{
		return GetSingle<T>(token, kind, kindPlural, out _);
	}
	private T? GetSingle<T>(ISyntaxToken token, string kind, string kindPlural, out T[] ambiguity)
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
	private void Get<T>(IConcreteSyntaxNode declaration, out T symbol) where T : notnull, IDeclaredSymbol
	{
		symbol = CurrentScope.Get<T>(declaration);
	}
	private void Update(IDeclaredSymbol symbol, IDeclaredSyntaxNode declaration)
	{
		CurrentScope.Update(symbol, declaration);
	}
	private void Update(IConcreteSyntaxNode oldDeclaration, IDeclaredSyntaxNode newDeclaration)
	{
		CurrentScope.UpdateChild(oldDeclaration, newDeclaration);
	}
	#endregion
}
