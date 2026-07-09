using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;

public interface IControlFlowIncomingBranch : IControlFlowBranch
{
	#region Properties
	IControlFlowBlock From { get; }
	bool IsReachable { get; }
	#endregion
}

public interface IMutableControlFlowIncomingBranch : IControlFlowIncomingBranch
{
	#region Properties
	new IMutableControlFlowBlock From { get; set; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IControlFlowBlock IControlFlowIncomingBranch.From => From;
	#endregion
}
