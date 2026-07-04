using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

public interface IControlFlowBlock
{
	#region Properties
	string Id { get; }
	IReadOnlyList<IControlFlowIncomingBranch> Incoming { get; }
	IReadOnlyList<IControlFlowOutgoingBranch> Outgoing { get; }
	#endregion
}

public interface IMutableControlFlowBlock : IControlFlowBlock
{
	#region Methods
	void AddIncoming(IControlFlowIncomingBranch incoming);
	void AddOutgoing(IControlFlowOutgoingBranch outgoing);
	#endregion
}

public abstract class MutableControlFlowBlock : IMutableControlFlowBlock
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<IControlFlowIncomingBranch> _incoming = [];

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<IControlFlowOutgoingBranch> _outgoing = [];
	#endregion

	#region Properties
	public abstract string Id { get; }
	public IReadOnlyList<IControlFlowIncomingBranch> Incoming => _incoming;
	public IReadOnlyList<IControlFlowOutgoingBranch> Outgoing => _outgoing;
	#endregion

	#region Methods
	public void AddIncoming(IControlFlowIncomingBranch incoming)
	{
		if (incoming is IControlFlowBidirectionalBranch branch && branch.To != this)
			ThrowHelper.ThrowArgumentException(nameof(incoming), $"Expected the incoming branch's target block ({branch.To}) to be this block ({this}).");

		_incoming.Add(incoming);
	}
	public void AddOutgoing(IControlFlowOutgoingBranch outgoing)
	{
		if (outgoing is IControlFlowBidirectionalBranch branch && branch.From != this)
			ThrowHelper.ThrowArgumentException(nameof(outgoing), $"Expected the outgoing branch's source block ({branch.From}) to be this block ({this}).");

		_outgoing.Add(outgoing);
	}
	#endregion
}
