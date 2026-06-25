namespace OwlDomain.Owl.Code.CodeAnalysis;

public sealed class CompilationUpdateResult : IStageResultPerformance, IStageResultParent
{
	#region Properties
	public string Stage => "compilation_update";
	public IPerformanceResult Performance { get; }
	public IReadOnlyCollection<IStageResult> Children { get; }
	#endregion

	#region Constructors
	public CompilationUpdateResult(
		IPerformanceResult performance,
		params IReadOnlyCollection<IStageResult> children)
	{
		Performance = performance;
		Children = children;
	}
	#endregion
}

public sealed class CompilationContext
{
	#region Fields
	private readonly Dictionary<ISourceFile, SyntaxTreeBundle> _trees = [];
	#endregion

	#region Properties
	public ISymbolScope BaseScope { get; }
	public IReadOnlyDictionary<ISourceFile, ISyntaxTreeBundle> Trees => _trees.ToDictionary(pair => pair.Key, pair => (ISyntaxTreeBundle)pair.Value);
	public IReadOnlyCollection<IConcreteSyntaxTree> Concrete => GetConcreteTrees().ToArray();
	public IReadOnlyCollection<IDeclaredSyntaxTree> Declared => GetDeclaredTrees().ToArray();
	public IReadOnlyCollection<ISemanticSyntaxTree> Semantic => GetSemanticTrees().ToArray();
	public IReadOnlyCollection<IAnnotatedSyntaxTree> Annotated => GetAnnotatedTrees().ToArray();
	#endregion

	#region Constructors
	public CompilationContext(ISymbolScope baseScope)
	{
		BaseScope = baseScope;
	}
	#endregion

	#region Methods
	public CompilationUpdateResult Update(
		IReadOnlyCollection<ISourceFile>? added = null,
		IReadOnlyCollection<ISourceFile>? removed = null,
		IReadOnlyCollection<ISourceFile>? changed = null)
	{
		added ??= [];
		removed ??= [];
		changed ??= [];

		using PerformanceScope _ = PerformanceResult.Scope(out IPerformanceResult performance);

		foreach (ISourceFile file in removed)
			_trees.Remove(file);

		foreach (ISourceFile file in added)
		{
			SyntaxTreeBundle bundle = new(file);
			_trees.Add(file, bundle);
		}

		HashSet<ISourceFile> toReparse = [.. added, .. changed];

		ParallelParsingResult parsing = Parser.Parse(toReparse);
		foreach (LexingAndParsingResult result in parsing.GetByFile().Values)
		{
			SyntaxTreeBundle bundle = _trees[result.Source];
			bundle.Concrete = result.Parsing.Tree;
		}

		SemanticResultGroup semanticGroup;
		using (PerformanceResult.Scope(out IPerformanceResult semanticPerformance))
		{
			DeclarationDiscoveryResult declarationDiscovery = DeclarationFinder.Discover(BaseScope, Concrete);
			ParallelDeclarationResolutionResult symbolResolution = SymbolResolver.Resolve(declarationDiscovery.ResultScope, Concrete);
			foreach (IDeclaredSyntaxTree tree in symbolResolution.Trees)
			{
				SyntaxTreeBundle bundle = _trees[tree.Source];
				bundle.Declared = tree;
			}

			ParallelSemanticResolutionResult semanticResolution = SemanticResolver.Resolve(declarationDiscovery.ResultScope, Declared);
			foreach (ISemanticSyntaxTree tree in semanticResolution.Trees)
			{
				SyntaxTreeBundle bundle = _trees[tree.Source];
				bundle.Semantic = tree;
			}

			semanticGroup = new(semanticPerformance, declarationDiscovery, symbolResolution, semanticResolution);
		}

		return new(performance, parsing, semanticGroup);
	}
	#endregion

	#region Helpers
	private IEnumerable<IConcreteSyntaxTree> GetConcreteTrees()
	{
		foreach (ISyntaxTreeBundle bundle in _trees.Values)
		{
			if (bundle.Concrete is null)
				ThrowHelper.ThrowInvalidOperationException("Expected the concrete tree to be set.");

			yield return bundle.Concrete;
		}
	}
	private IEnumerable<IDeclaredSyntaxTree> GetDeclaredTrees()
	{
		foreach (ISyntaxTreeBundle bundle in _trees.Values)
		{
			if (bundle.Declared is null)
				ThrowHelper.ThrowInvalidOperationException("Expected the declared tree to be set.");

			yield return bundle.Declared;
		}
	}
	private IEnumerable<ISemanticSyntaxTree> GetSemanticTrees()
	{
		foreach (ISyntaxTreeBundle bundle in _trees.Values)
		{
			if (bundle.Semantic is null)
				ThrowHelper.ThrowInvalidOperationException("Expected the semantic tree to be set.");

			yield return bundle.Semantic;
		}
	}
	private IEnumerable<IAnnotatedSyntaxTree> GetAnnotatedTrees()
	{
		foreach (ISyntaxTreeBundle bundle in _trees.Values)
		{
			if (bundle.Annotated is null)
				ThrowHelper.ThrowInvalidOperationException("Expected the annotated tree to be set.");

			yield return bundle.Annotated;
		}
	}
	#endregion
}
