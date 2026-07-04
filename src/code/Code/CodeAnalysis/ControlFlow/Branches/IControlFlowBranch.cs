using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;

public interface IControlFlowBranch
{
	#region Properties
	bool HasCondition { get; }
	IAnnotatedExpressionSyntax? Condition { get; }
	bool IsNegated { get; }
	#endregion
}

public sealed class ConditionalControlFlowBranch : IControlFlowBidirectionalBranch
{
	#region Properties
	public bool HasCondition => true;
	public IAnnotatedExpressionSyntax Condition { get; }
	public bool IsNegated { get; }
	public IControlFlowBlock From { get; }
	public IControlFlowBlock To { get; }
	#endregion

	#region Constructors
	public ConditionalControlFlowBranch(
		IAnnotatedExpressionSyntax condition,
		bool isNegated,
		IControlFlowBlock from,
		IControlFlowBlock to)
	{
		Condition = condition;
		IsNegated = isNegated;
		From = from;
		To = to;
	}
	#endregion
}

public sealed class UnconditionalControlFlowBranch : IControlFlowBidirectionalBranch
{
	#region Properties
	public bool HasCondition => false;
	public IAnnotatedExpressionSyntax? Condition => null;
	public bool IsNegated => default;

	public IControlFlowBlock From { get; }
	public IControlFlowBlock To { get; }
	#endregion

	#region Constructors
	public UnconditionalControlFlowBranch(IControlFlowBlock from, IControlFlowBlock to)
	{
		From = from;
		To = to;
	}
	#endregion
}
