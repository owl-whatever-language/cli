using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;

public interface IControlFlowOutgoingBranch : IControlFlowBranch
{
	#region Properties
	IControlFlowBlock To { get; }
	#endregion
}
