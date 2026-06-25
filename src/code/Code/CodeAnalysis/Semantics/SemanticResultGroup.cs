namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics;

public class SemanticResultGroup : IOrderedStageResultParent, IStageResultPerformance
{
	#region Properties
	public string Stage => "semantics";
	public IPerformanceResult Performance { get; }
	public IReadOnlyList<IStageResult> Children { get; }
	#endregion

	#region Constructors
	public SemanticResultGroup(IPerformanceResult performance, params IReadOnlyList<IStageResult> children)
	{
		Performance = performance;
		Children = children;
	}
	#endregion
}
