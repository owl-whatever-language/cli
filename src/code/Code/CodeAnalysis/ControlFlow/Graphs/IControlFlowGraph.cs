using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;
using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Graphs;

public interface IControlFlowGraph
{
	#region Properties
	IAnnotatedSyntaxNode Node { get; }
	IControlFlowStartBlock Start { get; }
	IControlFlowEndBlock End { get; }
	IReadOnlyList<IControlFlowBlock> Blocks { get; }
	IReadOnlyCollection<IControlFlowBidirectionalBranch> Branches { get; }
	#endregion
}

public interface IMutableControlFlowGraph : IControlFlowGraph
{
	#region Properties
	new IMutableControlFlowStartBlock Start { get; }
	new ControlFlowEndBlock End { get; }

	new IReadOnlyList<IMutableControlFlowBlock> Blocks { get; }
	new IReadOnlyCollection<IMutableControlFlowBidirectionalBranch> Branches { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IControlFlowStartBlock IControlFlowGraph.Start => Start;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IControlFlowEndBlock IControlFlowGraph.End => End;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyList<IControlFlowBlock> IControlFlowGraph.Blocks => Blocks;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyCollection<IControlFlowBidirectionalBranch> IControlFlowGraph.Branches => Branches;
	#endregion

	#region Methods
	void Add(IMutableControlFlowBlock block);
	void Add(IMutableControlFlowBidirectionalBranch branch);
	void RecalculateBlockNumbers();
	#endregion
}

public abstract class ControlFlowGraph<TNode> : IMutableControlFlowGraph
	where TNode : notnull, IAnnotatedSyntaxNode
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<IMutableControlFlowBlock> _blocks = [];

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly HashSet<IMutableControlFlowBidirectionalBranch> _branches = [];
	#endregion

	#region Properties
	public TNode Node { get; }
	public IMutableControlFlowStartBlock Start { get; }
	public ControlFlowEndBlock End { get; }
	public IReadOnlyList<IMutableControlFlowBlock> Blocks => _blocks;
	public IReadOnlyCollection<IMutableControlFlowBidirectionalBranch> Branches => _branches;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IAnnotatedSyntaxNode IControlFlowGraph.Node => Node;
	#endregion

	#region Constructors
	protected ControlFlowGraph(TNode node)
	{
		Node = node;
		Start = new ControlFlowStartBlock(this) { BlockNumber = 0 };
		End = new ControlFlowEndBlock() { BlockNumber = 1 };
	}
	#endregion

	#region Methods
	public void Add(IMutableControlFlowBlock block)
	{
		_blocks.Add(block);

		block.BlockNumber = _blocks.Count;
		End.BlockNumber = _blocks.Count + 1;
	}
	public void Add(IMutableControlFlowBidirectionalBranch branch) => _branches.Add(branch);
	public void RecalculateBlockNumbers()
	{
		Start.BlockNumber = 0;

		for (int i = 0; i < _blocks.Count; i++)
			_blocks[i].BlockNumber = i + 1;

		End.BlockNumber = _blocks.Count + 1;
	}
	#endregion
}
