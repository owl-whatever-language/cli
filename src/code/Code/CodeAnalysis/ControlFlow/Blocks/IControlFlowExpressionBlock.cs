namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

public interface IControlFlowExpressionBlock : IControlFlowBlock
{
	#region Properties
	IAnnotatedExpressionSyntax Expression { get; }
	#endregion
}

public sealed class ControlFlowExpressionBlock : MutableControlFlowBlock, IControlFlowExpressionBlock
{
	#region Properties
	public int BlockNumber { get; }
	public override string Id => $"{Expression.NodeKind.WithGroup}#{BlockNumber}";
	public IAnnotatedExpressionSyntax Expression { get; }
	#endregion

	#region Constructors
	public ControlFlowExpressionBlock(int blockNumber, IAnnotatedExpressionSyntax expression)
	{
		BlockNumber = blockNumber;
		Expression = expression;
	}
	#endregion
}
