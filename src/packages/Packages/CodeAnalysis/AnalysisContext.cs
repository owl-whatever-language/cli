namespace OwlDomain.Owl.Packages.CodeAnalysis;

public interface IAnalysisContext
{
	#region Properties
	IReadOnlyCollection<ISyntaxTreeBundle> Bundles { get; }
	#endregion
}

public interface IMutableAnalysisContext : IAnalysisContext
{
	#region Methods
	AnalysisUpdateResult Update(AnalysisUpdate update);
	#endregion
}

public static class IAnalysisContextExtensions
{
	extension(IAnalysisContext context)
	{
		#region Properties
		public IReadOnlyCollection<IConcreteSyntaxTree> Trees => context.Bundles.GetAvailableTrees().ToArray();
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
}


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
		IPerformanceResult performance)
	{
		Diagnostics = diagnostics;
		Performance = performance;
		Children = [];
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
	#endregion
}

public sealed class AnalysisContext : IMutableAnalysisContext
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly Dictionary<ISourceFile, SyntaxTreeBundle> _bundles = [];
	private readonly Dictionary<ISourceFile, IDiagnosticBag> _parsingDiagnostics = [];
	#endregion

	#region Properties
	public IReadOnlyCollection<ISyntaxTreeBundle> Bundles => _bundles.Values;
	public IEnumerable<IConcreteSyntaxTree> Trees => Bundles.GetAvailableTrees();
	public IReadOnlyCollection<IConcreteSyntaxTree> Concrete => Bundles.GetConcreteTrees().ToArray();
	#endregion

	#region Constructors
	public AnalysisContext() { }
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
		DiagnosticBag diagnostics = _parsingDiagnostics.Values.Combine();

		return new(diagnostics, performance);
	}
	#endregion
}
