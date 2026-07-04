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
	public IAnnotatedExpressionSyntax Expression { get; }
	#endregion

	#region Constructors
	public ControlFlowExpressionBlock(IAnnotatedExpressionSyntax expression)
	{
		Expression = expression;
	}
	#endregion
}
