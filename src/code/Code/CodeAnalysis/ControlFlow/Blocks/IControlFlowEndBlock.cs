using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

public interface IControlFlowEndBlock : IControlFlowBlock
{
	#region Properties
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyList<IControlFlowOutgoingBranch> IControlFlowBlock.Outgoing => [];
	#endregion
}

public sealed class ControlFlowEndBlock : IControlFlowEndBlock, IMutableControlFlowBlock
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<IControlFlowIncomingBranch> _incoming = [];
	#endregion

	#region Properties
	public string Id => "end_node";
	public IReadOnlyList<IControlFlowIncomingBranch> Incoming => _incoming;
	#endregion

	#region Methods
	public void AddIncoming(IControlFlowIncomingBranch incoming)
	{
		if (incoming is IControlFlowBidirectionalBranch branch && branch.To != this)
			ThrowHelper.ThrowArgumentException(nameof(incoming), $"Expected the incoming branch's target block ({branch.To}) to be this block ({this}).");

		_incoming.Add(incoming);
	}

	[DoesNotReturn]
	public void AddOutgoing(IControlFlowOutgoingBranch outgoing)
	{
		ThrowHelper.ThrowNotSupportedException($"The end control flow block cannot have any outgoing branches.");
	}
	#endregion
}
