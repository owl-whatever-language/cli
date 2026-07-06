using OwlDomain.ParsingTools.Syntax.Printing;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

public interface IControlFlowExpressionBlock : IControlFlowBlock
{
	#region Properties
	IAnnotatedExpressionSyntax Expression { get; }
	#endregion
}

public interface IMutableControlFlowExpressionBlock : IControlFlowExpressionBlock, IMutableControlFlowBlock
{
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class ControlFlowExpressionBlock : MutableControlFlowBlock, IMutableControlFlowExpressionBlock
{
	#region Properties
	public override string Id => $"{Expression.NodeKind.WithGroup}#{BlockNumber}";
	public IAnnotatedExpressionSyntax Expression { get; }
	#endregion

	#region Constructors
	public ControlFlowExpressionBlock(IAnnotatedExpressionSyntax expression)
	{
		Expression = expression;
	}
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"Block: {Id} | {Expression.GetDebugSource()}";
	#endregion
}
