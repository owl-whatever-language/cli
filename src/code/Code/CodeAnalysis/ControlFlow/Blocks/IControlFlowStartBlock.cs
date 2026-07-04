using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;
using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Graphs;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

public interface IControlFlowStartBlock : IControlFlowBlock
{
	#region Properties
	IControlFlowGraph Graph { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyList<IControlFlowIncomingBranch> IControlFlowBlock.Incoming => [];
	#endregion
}

public sealed class ControlFlowStartBlock : IControlFlowStartBlock, IMutableControlFlowBlock
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<IControlFlowOutgoingBranch> _outgoing = [];
	#endregion

	#region Properties
	public string Id => "start_node";
	public IControlFlowGraph Graph { get; }
	public IReadOnlyList<IControlFlowOutgoingBranch> Outgoing => _outgoing;
	#endregion

	#region Constructors
	public ControlFlowStartBlock(IControlFlowGraph graph) => Graph = graph;
	#endregion

	#region Methods
	[DoesNotReturn]
	void IMutableControlFlowBlock.AddIncoming(IControlFlowIncomingBranch incoming)
	{
		ThrowHelper.ThrowNotSupportedException($"The start control flow block cannot have any incoming branches.");
	}
	public void AddOutgoing(IControlFlowOutgoingBranch outgoing)
	{
		if (outgoing is IControlFlowBidirectionalBranch branch && branch.From != this)
			ThrowHelper.ThrowArgumentException(nameof(outgoing), $"Expected the outgoing branch's source block ({branch.From}) to be this block ({this}).");

		_outgoing.Add(outgoing);
	}
	#endregion
}
