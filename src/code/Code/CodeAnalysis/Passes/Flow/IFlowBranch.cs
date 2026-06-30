namespace OwlDomain.Owl.Code.CodeAnalysis.Passes.Flow;

public interface IFlowBranch
{
	#region Properties
	IFlowBlock From { get; }
	IFlowBlock To { get; }
	#endregion
}

public sealed class FlowBranch : IFlowBranch
{
	#region Properties
	public FlowBlock From { get; }
	public FlowBlock To { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IFlowBlock IFlowBranch.From => From;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IFlowBlock IFlowBranch.To => To;
	#endregion

	#region Constructors
	public FlowBranch(FlowBlock from, FlowBlock to)
	{
		From = from;
		To = to;
	}
	#endregion
}
