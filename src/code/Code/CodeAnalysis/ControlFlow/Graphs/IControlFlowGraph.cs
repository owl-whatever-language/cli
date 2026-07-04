using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Graphs;

public interface IControlFlowGraph
{
	#region Properties
	IAnnotatedSyntaxNode Node { get; }
	IControlFlowStartBlock Start { get; }
	IControlFlowEndBlock End { get; }
	#endregion
}

public interface IMutableControlFlowGraph : IControlFlowGraph
{
	#region Properties
	new ControlFlowStartBlock Start { get; }
	new ControlFlowEndBlock End { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IControlFlowStartBlock IControlFlowGraph.Start => Start;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IControlFlowEndBlock IControlFlowGraph.End => End;
	#endregion
}

public abstract class ControlFlowGraph<TNode> : IMutableControlFlowGraph
	where TNode : notnull, IAnnotatedSyntaxNode
{
	#region Properties
	public TNode Node { get; }
	public ControlFlowStartBlock Start { get; }
	public ControlFlowEndBlock End { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IAnnotatedSyntaxNode IControlFlowGraph.Node => Node;
	#endregion

	#region Constructors
	protected ControlFlowGraph(TNode node)
	{
		Node = node;
		Start = new(this);
		End = new();
	}
	#endregion
}
