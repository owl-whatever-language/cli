using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;

public interface IControlFlowOutgoingBranch : IControlFlowBranch
{
	#region Properties
	IControlFlowBlock To { get; }
	#endregion
}

public interface IMutableControlFlowOutgoingBranch : IControlFlowOutgoingBranch
{
	#region Properties
	new IMutableControlFlowBlock To { get; set; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IControlFlowBlock IControlFlowOutgoingBranch.To => To;
	#endregion
}
