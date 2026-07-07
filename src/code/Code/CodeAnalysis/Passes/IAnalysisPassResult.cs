namespace OwlDomain.Owl.Code.CodeAnalysis.Passes;

public interface IAnalysisPassResult : IStageResult, IStageResultDiagnostics, IStageResultPerformance
{
	#region Properties
	IAnalysisPass Pass { get; }
	#endregion
}

public class AnalysisPassResult : IAnalysisPassResult
{
	#region Properties
	public IAnalysisPass Pass { get; }
	public string Stage => Pass.Kind;
	public IPerformanceResult Performance { get; }
	public IDiagnosticBag Diagnostics { get; }
	#endregion

	#region Constructors
	public AnalysisPassResult(IAnalysisPass pass, IPerformanceResult performance, IDiagnosticBag diagnostics)
	{
		Pass = pass;
		Performance = performance;
		Diagnostics = diagnostics;
	}
	public AnalysisPassResult(IAnalysisPass pass, IPerformanceResult performance) : this(pass, performance, DiagnosticBag.Empty) { }
	#endregion
}

public sealed class AnalysisPassResultGroup : IOrderedStageResultParent<IAnalysisPassResult>, IStageResultPerformance
{
	#region Properties
	public string Stage => "analysis_passes";
	public IPerformanceResult Performance { get; }
	public IReadOnlyList<IAnalysisPassResult> Children { get; }
	#endregion

	#region Constructors
	public AnalysisPassResultGroup(IPerformanceResult performance, params IReadOnlyList<IAnalysisPassResult> children)
	{
		Performance = performance;
		Children = children;
	}
	#endregion
}

public sealed class AnalysisPassTreeResult : AnalysisPassResult, ISourceStageResult
{
	#region Properties
	public IAnnotatedSyntaxTree Tree { get; }
	public ISourceFile Source => Tree.Source;
	#endregion

	#region Constructors
	public AnalysisPassTreeResult(
		IAnalysisPass pass,
		IPerformanceResult performance,
		IDiagnosticBag diagnostics,
		IAnnotatedSyntaxTree tree)
		: base(pass, performance, diagnostics)
	{
		Tree = tree;
	}
	#endregion
}

public sealed class ParallelAnalysisPassTreeResult : AnalysisPassResult, IParallelStageResult<AnalysisPassTreeResult>
{
	#region Properties
	public IReadOnlyCollection<AnalysisPassTreeResult> Children { get; }
	#endregion

	#region Constructors
	public ParallelAnalysisPassTreeResult(
		IAnalysisPass pass,
		IPerformanceResult performance,
		IDiagnosticBag diagnostics,
		IReadOnlyCollection<AnalysisPassTreeResult> results)
		: base(pass, performance, diagnostics)
	{
		Children = results;
	}

	public ParallelAnalysisPassTreeResult(
		IAnalysisPass pass,
		IPerformanceResult performance,
		IReadOnlyCollection<AnalysisPassTreeResult> results)
		: base(pass, performance)
	{
		Children = results;
	}
	#endregion
}
