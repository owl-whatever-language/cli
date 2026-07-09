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

public interface IMutableControlFlowStartBlock : IControlFlowStartBlock, IMutableControlFlowBlock
{
	#region Properties
	new IMutableControlFlowGraph Graph { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IControlFlowGraph IControlFlowStartBlock.Graph => Graph;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyList<IControlFlowIncomingBranch> IControlFlowBlock.Incoming => [];

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyList<IMutableControlFlowIncomingBranch> IMutableControlFlowBlock.Incoming => [];
	#endregion
}

public sealed class ControlFlowStartBlock : IMutableControlFlowStartBlock
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<IMutableControlFlowOutgoingBranch> _outgoing = [];
	#endregion

	#region Properties
	public string Id => $"start_node#{BlockNumber}";
	public IMutableControlFlowGraph Graph { get; }
	public IReadOnlyList<IMutableControlFlowOutgoingBranch> Outgoing => _outgoing;
	public int BlockNumber { get; set; }
	#endregion

	#region Constructors
	public ControlFlowStartBlock(IMutableControlFlowGraph graph) => Graph = graph;
	#endregion

	#region Methods
	[DoesNotReturn]
	void IMutableControlFlowBlock.AddIncoming(IMutableControlFlowIncomingBranch incoming)
	{
		ThrowHelper.ThrowNotSupportedException($"The start control flow block cannot have any incoming branches.");
	}
	public void AddOutgoing(IMutableControlFlowOutgoingBranch outgoing)
	{
		if (outgoing is IControlFlowBidirectionalBranch branch && branch.From != this)
			ThrowHelper.ThrowArgumentException(nameof(outgoing), $"Expected the outgoing branch's source block ({branch.From}) to be this block ({this}).");

		_outgoing.Add(outgoing);
	}
	#endregion
}
