using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;
using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Graphs;

public interface IControlFlowGraph
{
	#region Properties
	IAnnotatedSyntaxNode Node { get; }
	IControlFlowStartBlock Start { get; }
	IControlFlowEndBlock End { get; }
	IReadOnlySet<IControlFlowBlock> Blocks { get; }
	IReadOnlySet<IControlFlowBidirectionalBranch> Branches { get; }
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

	#region Methods
	void Add(IControlFlowBlock block);
	void Add(IControlFlowBidirectionalBranch branch);
	#endregion
}

public abstract class ControlFlowGraph<TNode> : IMutableControlFlowGraph
	where TNode : notnull, IAnnotatedSyntaxNode
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly HashSet<IControlFlowBlock> _blocks = [];

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly HashSet<IControlFlowBidirectionalBranch> _branches = [];
	#endregion

	#region Properties
	public TNode Node { get; }
	public ControlFlowStartBlock Start { get; }
	public ControlFlowEndBlock End { get; }
	public IReadOnlySet<IControlFlowBlock> Blocks => _blocks;
	public IReadOnlySet<IControlFlowBidirectionalBranch> Branches => _branches;

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

	#region Methods
	public void Add(IControlFlowBlock block) => _blocks.Add(block);
	public void Add(IControlFlowBidirectionalBranch branch) => _branches.Add(branch);
	#endregion
}
