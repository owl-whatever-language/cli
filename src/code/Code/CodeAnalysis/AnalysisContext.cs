namespace OwlDomain.Owl.Code.CodeAnalysis;

public interface IAnalysisContext
{
	#region Properties
	IReadOnlyCollection<ISyntaxTreeBundle> Bundles { get; }
	#endregion
}

public interface IMutableAnalysisContext : IAnalysisContext
{
	#region Methods
	IMutableAnalysisContext RegisterPass(IAnalysisPass pass);
	AnalysisUpdateResult Update(AnalysisUpdate update);
	#endregion
}

public static class IAnalysisContextExtensions
{
	extension(IAnalysisContext context)
	{
		#region Properties
		public IReadOnlyCollection<ISourceFile> Sources => context.Bundles.Select(b => b.Source).ToArray();
		public IReadOnlyCollection<IConcreteSyntaxTree> Trees => context.Bundles.GetAvailableTrees().ToArray();
		public IReadOnlyCollection<IAnnotatedSyntaxTree> Annotated => context.Bundles.GetMostDetailedTrees().ToArray();
		#endregion

		#region Methods
		public bool TryGet(ISourceFile source, [NotNullWhen(true)] out ISyntaxTreeBundle? bundle)
		{
			foreach (ISyntaxTreeBundle current in context.Bundles)
			{
				if (current.Source == source)
				{
					bundle = current;
					return true;
				}
			}

			bundle = default;
			return false;
		}
		#endregion
	}
	extension(IMutableAnalysisContext context)
	{
		#region Methods
		public IMutableAnalysisContext RegisterPass<T>() where T : notnull, IAnalysisPass, new()
		{
			T pass = new();
			return context.RegisterPass(pass);
		}
		#endregion
	}
}


public sealed class AnalysisUpdateResult : IStageResultDiagnostics, IStageResultPerformance, IStageResultParent
{
	#region Properties
	public string Stage => "analysis_update";
	public IDiagnosticBag Diagnostics { get; }
	public IPerformanceResult Performance { get; }
	public IReadOnlyCollection<IStageResult> Children { get; }
	public ParallelParsingResult Parsing { get; }
	public SemanticResultGroup Semantics { get; }
	public ParallelAnnotationPreparingResult Annotations { get; }
	public AnalysisPassResultGroup? Passes { get; }
	#endregion

	#region Constructors
	public AnalysisUpdateResult(
		IDiagnosticBag diagnostics,
		IPerformanceResult performance,
		ParallelParsingResult parsing,
		SemanticResultGroup semantics,
		ParallelAnnotationPreparingResult annotations,
		AnalysisPassResultGroup? passes)
	{
		Diagnostics = diagnostics;
		Performance = performance;

		Children = passes is not null ? [parsing, semantics, annotations, passes] : [parsing, semantics, annotations];

		Parsing = parsing;
		Semantics = semantics;
		Annotations = annotations;
		Passes = passes;
	}
	#endregion
}

public readonly struct AnalysisUpdate
{
	#region Properties
	public IReadOnlyCollection<ISourceFile> Removed
	{
		get => field ?? [];
		init;
	}
	public IReadOnlyCollection<ISourceFile> Added
	{
		get => field ?? [];
		init;
	}
	public IReadOnlyCollection<ISourceFile> Changed
	{
		get => field ?? [];
		init;
	}
	public bool IsEmpty
	{
		get
		{
			return
				Removed.Count is 0 &&
				Added.Count is 0 &&
				Changed.Count is 0
			;
		}
	}
	#endregion
}

public sealed class AnalysisContext : IMutableAnalysisContext
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly Dictionary<ISourceFile, SyntaxTreeBundle> _bundles = [];
	private readonly Dictionary<ISourceFile, IDiagnosticBag> _parsingDiagnostics = [];
	private readonly List<IAnalysisPass> _passes = [];
	#endregion

	#region Properties
	public ISymbolScope BaseScope { get; }
	public IReadOnlyCollection<ISyntaxTreeBundle> Bundles => _bundles.Values;
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

		// Note(Nightowl): Preamble;
		this.RegisterPass<ControlFlow.ControlFlowAnalyser>();

		// Note(Nightowl): Annotators;
		this.RegisterPass<Passes.LocalCapture.LocalCaptureAnnotator>();

		// Note(Nightowl): Checkers;
		this.RegisterPass<Passes.LocalCapture.LocalCaptureChecker>();
		this.RegisterPass<Passes.EntryPoint.EntryPointAnalyser>();
	}
	#endregion

	#region Methods
	public AnalysisUpdateResult Update(AnalysisUpdate update)
	{
		using PerformanceScope _ = PerformanceResult.Scope(out IPerformanceResult performance);

		foreach (ISourceFile file in update.Removed)
		{
			_bundles.Remove(file);
			_parsingDiagnostics.Remove(file);
		}

		foreach (ISourceFile file in update.Added)
		{
			SyntaxTreeBundle bundle = new(file);
			_bundles.Add(file, bundle);
		}

		HashSet<ISourceFile> toReparse = [.. update.Added, .. update.Changed];
		ParallelParsingResult parsing = Parse(toReparse);
		SemanticResultGroup semantics = RunSemanticGroup(out ISymbolScope userScope);
		ParallelAnnotationPreparingResult annotations = PrepareAnnotations(userScope);
		AnalysisPassResultGroup? passes = TryRunPasses();

		DiagnosticBag diagnostics = _parsingDiagnostics.Values.Combine();

		// Note(Nightowl): Add diagnostics for the next update, to make sure we don't duplicate them for this update;
		foreach (LexingAndParsingResult result in parsing.GetByFile().Values)
			_parsingDiagnostics.Add(result.Source, result.GetAllDiagnostics());

		return new(diagnostics, performance, parsing, semantics, annotations, passes);
	}

	private ParallelParsingResult Parse(IReadOnlyCollection<ISourceFile> files)
	{
		ParallelParsingResult result = Parser.Parse(files);
		foreach (LexingAndParsingResult current in result.GetByFile().Values)
		{
			_parsingDiagnostics.Remove(current.Source);

			SyntaxTreeBundle bundle = _bundles[current.Source];
			bundle.Concrete = current.Parsing.Tree;
		}

		return result;
	}
	private DeclarationDiscoveryResult DiscoverDeclarations(out ISymbolScope userScope)
	{
		DeclarationDiscoveryResult result = DeclarationFinder.Discover(BaseScope, Concrete);
		userScope = result.ResultScope;

		return result;
	}
	private ParallelDeclarationResolutionResult ResolveSymbols(ISymbolScope userScope)
	{
		ParallelDeclarationResolutionResult result = DeclarationResolver.Resolve(userScope, Concrete);
		foreach (IDeclaredSyntaxTree tree in result.Trees)
		{
			SyntaxTreeBundle bundle = _bundles[tree.Source];
			bundle.Declared = tree;
		}

		return result;
	}
	private ParallelSemanticResolutionResult ResolveSemantics(ISymbolScope userScope)
	{
		ParallelSemanticResolutionResult result = SemanticResolver.Resolve(userScope, Declared);
		foreach (ISemanticSyntaxTree tree in result.Trees)
		{
			SyntaxTreeBundle bundle = _bundles[tree.Source];
			bundle.Semantic = tree;
		}

		return result;
	}
	private SemanticResultGroup RunSemanticGroup(out ISymbolScope userScope)
	{
		using (PerformanceResult.Scope(out IPerformanceResult semanticPerformance))
		{
			DeclarationDiscoveryResult declarations = DiscoverDeclarations(out userScope);
			ParallelDeclarationResolutionResult symbols = ResolveSymbols(userScope);
			ParallelSemanticResolutionResult semantics = ResolveSemantics(userScope);

			return new(semanticPerformance, declarations, symbols, semantics);
		}
	}
	private ParallelAnnotationPreparingResult PrepareAnnotations(ISymbolScope userScope)
	{
		ParallelAnnotationPreparingResult result = AnnotationPreparer.Prepare(userScope, Semantic);
		foreach (IAnnotatedSyntaxTree tree in result.Trees)
		{
			SyntaxTreeBundle bundle = _bundles[tree.Source];
			bundle.Annotated = tree;
		}

		return result;
	}
	#endregion

	#region Pass methods
	public IMutableAnalysisContext RegisterPass(IAnalysisPass pass)
	{
		_passes.Add(pass);
		return this;
	}
	private AnalysisPassResultGroup? TryRunPasses()
	{
		if (_passes.Any())
			return RunPasses();

		return default;
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
