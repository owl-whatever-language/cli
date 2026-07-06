namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;

public interface IControlFlowBidirectionalBranch : IControlFlowIncomingBranch, IControlFlowOutgoingBranch
{

}

public interface IMutableControlFlowBidirectionalBranch :
	IControlFlowBidirectionalBranch,
	IMutableControlFlowIncomingBranch,
	IMutableControlFlowOutgoingBranch
{
}
