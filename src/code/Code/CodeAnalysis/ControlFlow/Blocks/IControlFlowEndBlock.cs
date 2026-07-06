using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

public interface IControlFlowEndBlock : IControlFlowBlock
{
	#region Properties
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyList<IControlFlowOutgoingBranch> IControlFlowBlock.Outgoing => [];
	#endregion
}

public interface IMutableControlFlowEndBlock : IControlFlowEndBlock, IMutableControlFlowBlock
{
	#region Properties
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyList<IMutableControlFlowOutgoingBranch> IMutableControlFlowBlock.Outgoing => [];

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyList<IControlFlowOutgoingBranch> IControlFlowBlock.Outgoing => [];
	#endregion
}

public sealed class ControlFlowEndBlock : IMutableControlFlowEndBlock
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<IMutableControlFlowIncomingBranch> _incoming = [];
	#endregion

	#region Properties
	public int BlockNumber { get; set; }
	public string Id => $"end_node#{BlockNumber}";
	public IReadOnlyList<IMutableControlFlowIncomingBranch> Incoming => _incoming;
	#endregion

	#region Methods
	public void AddIncoming(IMutableControlFlowIncomingBranch incoming)
	{
		if (incoming is IControlFlowBidirectionalBranch branch && branch.To != this)
			ThrowHelper.ThrowArgumentException(nameof(incoming), $"Expected the incoming branch's target block ({branch.To}) to be this block ({this}).");

		_incoming.Add(incoming);
	}

	[DoesNotReturn]
	public void AddOutgoing(IMutableControlFlowOutgoingBranch outgoing)
	{
		ThrowHelper.ThrowNotSupportedException($"The end control flow block cannot have any outgoing branches.");
	}
	#endregion
}
