using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;
using OwlDomain.ParsingTools.Syntax.Printing;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;

public interface IControlFlowBranch
{
	#region Properties
	bool HasCondition { get; }
	IAnnotatedExpressionSyntax? Condition { get; }
	bool IsNegated { get; }
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class ConditionalControlFlowBranch : IMutableControlFlowBidirectionalBranch
{
	#region Properties
	public bool HasCondition => true;
	public IAnnotatedExpressionSyntax Condition { get; }
	public bool IsNegated { get; }
	public IMutableControlFlowBlock From { get; set; }
	public IMutableControlFlowBlock To { get; set; }
	#endregion

	#region Constructors
	public ConditionalControlFlowBranch(
		IAnnotatedExpressionSyntax condition,
		bool isNegated,
		IMutableControlFlowBlock from,
		IMutableControlFlowBlock to)
	{
		Condition = condition;
		IsNegated = isNegated;
		From = from;
		To = to;
	}
	#endregion

	#region Helpers
	private string DebuggerDisplay()
	{
		if (IsNegated)
			return $"Branch: {From.Id} -> {To.Id} | NOT {Condition.GetDebugSource()}";

		return $"Branch: {From.Id} -> {To.Id} | {Condition.GetDebugSource()}";
	}
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class UnconditionalControlFlowBranch : IMutableControlFlowBidirectionalBranch
{
	#region Properties
	public bool HasCondition => false;
	public IAnnotatedExpressionSyntax? Condition => null;
	public bool IsNegated => default;

	public IMutableControlFlowBlock From { get; set; }
	public IMutableControlFlowBlock To { get; set; }
	#endregion

	#region Constructors
	public UnconditionalControlFlowBranch(IMutableControlFlowBlock from, IMutableControlFlowBlock to)
	{
		From = from;
		To = to;
	}
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"Branch: {From.Id} -> {To.Id}";
	#endregion
}
