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
	public IEnumerable<IConcreteSyntaxTree> Trees => Bundles.GetAvailableTrees();
	public IReadOnlyCollection<IConcreteSyntaxTree> Concrete => Bundles.GetConcreteTrees().ToArray();
	public IReadOnlyCollection<IDeclaredSyntaxTree> Declared => Bundles.GetDeclaredTrees().ToArray();
	public IReadOnlyCollection<ISemanticSyntaxTree> Semantic => Bundles.GetSemanticTrees().ToArray();
	public IReadOnlyCollection<IAnnotatedSyntaxTree> Annotated => Bundles.GetAnnotatedTrees().ToArray();
	#endregion

	#region Constructors
	public AnalysisContext(ISymbolScope baseScope)
	{
		BaseScope = baseScope;

		RegisterPass<ControlFlow.ControlFlowAnalyser>();
		RegisterPass<Passes.LocalCapture.LocalCaptureAnalysis>();
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
		ParallelParsingResult parsing = Parse(stageCompleteCallback, toReparse);
		SemanticResultGroup semantics = RunSemanticGroup(stageCompleteCallback, out ISymbolScope userScope);
		ParallelAnnotationPreparingResult annotations = PrepareAnnotations(stageCompleteCallback, userScope);

		List<IStageResult> results = [parsing, semantics, annotations];

		if (TryRunPasses(out AnalysisPassResultGroup? passResults))
			results.Add(passResults);

		DiagnosticBag diagnostics = _parsingDiagnostics.Values.Combine();

		// Note(Nightowl): Add diagnostics for the next update, to make sure we don't duplicate them for this update;
		foreach (LexingAndParsingResult result in parsing.GetByFile().Values)
			_parsingDiagnostics.Add(result.Source, result.GetAllDiagnostics());

		return new(diagnostics, performance, results);
	}
	private ParallelParsingResult Parse(AnalysisStageCompleteDelegate? callback, IReadOnlyCollection<ISourceFile> files)
	{
		ParallelParsingResult result = Parser.Parse(files);
		foreach (LexingAndParsingResult current in result.GetByFile().Values)
		{
			_parsingDiagnostics.Remove(current.Source);

			SyntaxTreeBundle bundle = _trees[current.Source];
			bundle.Concrete = current.Parsing.Tree;
		}

		callback?.Invoke(this, result);
		return result;
	}
	private DeclarationDiscoveryResult DiscoverDeclarations(AnalysisStageCompleteDelegate? callback, out ISymbolScope userScope)
	{
		DeclarationDiscoveryResult result = DeclarationFinder.Discover(BaseScope, Concrete);
		userScope = result.ResultScope;

		callback?.Invoke(this, result);
		return result;
	}
	private ParallelDeclarationResolutionResult ResolveSymbols(AnalysisStageCompleteDelegate? callback, ISymbolScope userScope)
	{
		ParallelDeclarationResolutionResult result = SymbolResolver.Resolve(userScope, Concrete);
		foreach (IDeclaredSyntaxTree tree in result.Trees)
		{
			SyntaxTreeBundle bundle = _trees[tree.Source];
			bundle.Declared = tree;
		}

		callback?.Invoke(this, result);
		return result;
	}
	private ParallelSemanticResolutionResult ResolveSemantics(AnalysisStageCompleteDelegate? callback, ISymbolScope userScope)
	{
		ParallelSemanticResolutionResult result = SemanticResolver.Resolve(userScope, Declared);
		foreach (ISemanticSyntaxTree tree in result.Trees)
		{
			SyntaxTreeBundle bundle = _trees[tree.Source];
			bundle.Semantic = tree;
		}

		callback?.Invoke(this, result);
		return result;
	}
	private SemanticResultGroup RunSemanticGroup(AnalysisStageCompleteDelegate? callback, out ISymbolScope userScope)
	{
		SemanticResultGroup result;
		using (PerformanceResult.Scope(out IPerformanceResult semanticPerformance))
		{
			DeclarationDiscoveryResult declarations = DiscoverDeclarations(callback, out userScope);
			ParallelDeclarationResolutionResult symbols = ResolveSymbols(callback, userScope);
			ParallelSemanticResolutionResult semantics = ResolveSemantics(callback, userScope);

			result = new(semanticPerformance, declarations, symbols, semantics);
		}

		callback?.Invoke(this, result);
		return result;
	}
	private ParallelAnnotationPreparingResult PrepareAnnotations(AnalysisStageCompleteDelegate? callback, ISymbolScope userScope)
	{
		ParallelAnnotationPreparingResult result = AnnotationPreparer.Prepare(userScope, Semantic);
		foreach (IAnnotatedSyntaxTree tree in result.Trees)
		{
			SyntaxTreeBundle bundle = _trees[tree.Source];
			bundle.Annotated = tree;
		}

		callback?.Invoke(this, result);
		return result;
	}
	private bool TryRunPasses([NotNullWhen(true)] out AnalysisPassResultGroup? result)
	{
		if (_passes.Any())
		{
			result = RunPasses();
			return true;
		}

		result = default;
		return false;
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
}
