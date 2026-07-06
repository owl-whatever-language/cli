using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Graphs;
using OwlDomain.ParsingTools.Syntax.Printing;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

public interface IControlFlowExpressionBlock : IControlFlowBlock
{
	#region Properties
	IAnnotatedExpressionSyntax Expression { get; }
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class ControlFlowExpressionBlock : MutableControlFlowBlock, IControlFlowExpressionBlock
{
	#region Properties
	public int BlockNumber { get; }
	public override string Id => $"{Expression.NodeKind.WithGroup}#{BlockNumber}";
	public IAnnotatedExpressionSyntax Expression { get; }
	#endregion

	#region Constructors
	public ControlFlowExpressionBlock(IMutableControlFlowGraph graph, IAnnotatedExpressionSyntax expression)
	{
		BlockNumber = graph.Blocks.Count + 1;
		Expression = expression;

		graph.Add(this);
	}
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"Block: {Id} | {Expression.GetDebugSource()}";
	#endregion
}
