namespace OwlDomain.Owl.Code.CodeAnalysis;

public sealed class AnalysisUpdateResult : IStageResultDiagnostics, IStageResultPerformance, IStageResultParent
{
	#region Properties
	public string Stage => "analysis_update";
	public IDiagnosticBag Diagnostics { get; }
	public IPerformanceResult Performance { get; }
	public IReadOnlyCollection<IStageResult> Children { get; }
	#endregion

	#region Constructors
	public AnalysisUpdateResult(
		IDiagnosticBag diagnostics,
		IPerformanceResult performance,
		params IReadOnlyCollection<IStageResult> children)
	{
		Diagnostics = diagnostics;
		Performance = performance;
		Children = children;
	}
	#endregion
}

public delegate void AnalysisStageCompleteDelegate(AnalysisContext context, IStageResult result);

public interface IAnalysisContext
{
	#region Properties
	IReadOnlyCollection<IAnnotatedSyntaxTree> Annotated { get; }
	#endregion
}

public sealed class AnalysisContext : IAnalysisContext
{
	#region Fields
	private readonly Dictionary<ISourceFile, SyntaxTreeBundle> _trees = [];
	private readonly Dictionary<ISourceFile, IDiagnosticBag> _parsingDiagnostics = [];
	private readonly List<IAnalysisPass> _passes = [];
	#endregion

	#region Properties
	public ISymbolScope BaseScope { get; }
	public IEnumerable<ISyntaxTreeBundle> Bundles => _trees.Values;
	public IEnumerable<IConcreteSyntaxTree> Trees => Bundles.Where(b => b.LeastDetailed is not null).Select(b => b.LeastDetailed)!;
	public IReadOnlyCollection<IConcreteSyntaxTree> Concrete => GetConcreteTrees().ToArray();
	public IReadOnlyCollection<IDeclaredSyntaxTree> Declared => GetDeclaredTrees().ToArray();
	public IReadOnlyCollection<ISemanticSyntaxTree> Semantic => GetSemanticTrees().ToArray();
	public IReadOnlyCollection<IAnnotatedSyntaxTree> Annotated => GetAnnotatedTrees().ToArray();
	#endregion

	#region Constructors
	public AnalysisContext(ISymbolScope baseScope)
	{
		BaseScope = baseScope;

		RegisterPass<ControlFlow.ControlFlowAnalyser>();
	}
	#endregion

	#region Methods
	public AnalysisContext RegisterPass(IAnalysisPass pass)
	{
		_passes.Add(pass);
		return this;
	}
	public AnalysisContext RegisterPass<T>() where T : notnull, IAnalysisPass, new()
	{
		T pass = new();
		_passes.Add(pass);

		return this;
	}

	public AnalysisUpdateResult Update(
		IReadOnlyCollection<ISourceFile>? added = null,
		IReadOnlyCollection<ISourceFile>? removed = null,
		IReadOnlyCollection<ISourceFile>? changed = null,
		AnalysisStageCompleteDelegate? stageCompleteCallback = null)
	{
		added ??= [];
		removed ??= [];
		changed ??= [];

		using PerformanceScope _ = PerformanceResult.Scope(out IPerformanceResult performance);

		foreach (ISourceFile file in removed)
		{
			_trees.Remove(file);
			_parsingDiagnostics.Remove(file);
		}

		foreach (ISourceFile file in added)
		{
			SyntaxTreeBundle bundle = new(file);
			_trees.Add(file, bundle);
		}

		HashSet<ISourceFile> toReparse = [.. added, .. changed];

		ParallelParsingResult parsing = Parser.Parse(toReparse);
		foreach (LexingAndParsingResult result in parsing.GetByFile().Values)
		{
			_parsingDiagnostics.Remove(result.Source);

			SyntaxTreeBundle bundle = _trees[result.Source];
			bundle.Concrete = result.Parsing.Tree;
		}
		stageCompleteCallback?.Invoke(this, parsing);

		SemanticResultGroup semanticGroup;
		ISymbolScope userScope;
		using (PerformanceResult.Scope(out IPerformanceResult semanticPerformance))
		{
			DeclarationDiscoveryResult declarationDiscovery = DeclarationFinder.Discover(BaseScope, Concrete);
			userScope = declarationDiscovery.ResultScope;
			stageCompleteCallback?.Invoke(this, declarationDiscovery);

			ParallelDeclarationResolutionResult symbolResolution = SymbolResolver.Resolve(userScope, Concrete);
			foreach (IDeclaredSyntaxTree tree in symbolResolution.Trees)
			{
				SyntaxTreeBundle bundle = _trees[tree.Source];
				bundle.Declared = tree;
			}
			stageCompleteCallback?.Invoke(this, symbolResolution);

			ParallelSemanticResolutionResult semanticResolution = SemanticResolver.Resolve(userScope, Declared);
			foreach (ISemanticSyntaxTree tree in semanticResolution.Trees)
			{
				SyntaxTreeBundle bundle = _trees[tree.Source];
				bundle.Semantic = tree;
			}
			stageCompleteCallback?.Invoke(this, semanticResolution);

			semanticGroup = new(semanticPerformance, declarationDiscovery, symbolResolution, semanticResolution);
		}

		stageCompleteCallback?.Invoke(this, semanticGroup);

		ParallelAnnotationPreparingResult annotationPreparing = AnnotationPreparer.Prepare(userScope, Semantic);
		foreach (IAnnotatedSyntaxTree tree in annotationPreparing.Trees)
		{
			SyntaxTreeBundle bundle = _trees[tree.Source];
			bundle.Annotated = tree;
		}
		stageCompleteCallback?.Invoke(this, annotationPreparing);

		List<IStageResult> results = [parsing, semanticGroup, annotationPreparing];

		if (_passes.Any())
		{
			AnalysisPassResultGroup passResult = RunPasses();
			results.Add(passResult);
		}

		DiagnosticBag diagnostics = _parsingDiagnostics.Values.Combine();

		// Note(Nightowl): Ad diagnostics for the next update, to make sure we don't duplicate them for this update;
		foreach (LexingAndParsingResult result in parsing.GetByFile().Values)
			_parsingDiagnostics.Add(result.Source, result.GetAllDiagnostics());

		return new(diagnostics, performance, results);
	}
	private AnalysisPassResultGroup RunPasses()
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			List<IAnalysisPassResult> results = [];

			foreach (IAnalysisPass pass in _passes)
			{
				IAnalysisPassResult result = pass.Run(this);
				results.Add(result);
			}

			return new(performance, results);
		}
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
