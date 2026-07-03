namespace OwlDomain.Owl.Code.CodeAnalysis.Passes.ControlFlow;

public interface IControlFlowBranch
{
	#region Properties
	IControlFlowBlock From { get; }
	IControlFlowBlock To { get; }
	#endregion
}

public sealed class ControlFlowBranch : IControlFlowBranch
{
	#region Properties
	public ControlFlowBlock From { get; }
	public ControlFlowBlock To { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IControlFlowBlock IControlFlowBranch.From => From;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IControlFlowBlock IControlFlowBranch.To => To;
	#endregion

	#region Constructors
	public ControlFlowBranch(ControlFlowBlock from, ControlFlowBlock to)
	{
		From = from;
		To = to;
	}
	#endregion
}
