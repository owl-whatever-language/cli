using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;

public interface IControlFlowIncomingBranch : IControlFlowBranch
{
	#region Properties
	IControlFlowBlock From { get; }
	#endregion
}
