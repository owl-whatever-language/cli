using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

public interface IControlFlowBlock
{
	#region Properties
	int BlockNumber { get; }
	string Id { get; }
	IReadOnlyList<IControlFlowIncomingBranch> Incoming { get; }
	IReadOnlyList<IControlFlowOutgoingBranch> Outgoing { get; }
	#endregion
}

public interface IMutableControlFlowBlock : IControlFlowBlock
{
	#region Properties
	new int BlockNumber { get; set; }
	new IReadOnlyList<IMutableControlFlowIncomingBranch> Incoming { get; }
	new IReadOnlyList<IMutableControlFlowOutgoingBranch> Outgoing { get; }

	IReadOnlyList<IControlFlowIncomingBranch> IControlFlowBlock.Incoming => Incoming;
	IReadOnlyList<IControlFlowOutgoingBranch> IControlFlowBlock.Outgoing => Outgoing;
	#endregion

	#region Methods
	void AddIncoming(IMutableControlFlowIncomingBranch incoming);
	void AddOutgoing(IMutableControlFlowOutgoingBranch outgoing);
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public abstract class MutableControlFlowBlock : IMutableControlFlowBlock
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<IMutableControlFlowIncomingBranch> _incoming = [];

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<IMutableControlFlowOutgoingBranch> _outgoing = [];
	#endregion

	#region Properties
	public int BlockNumber { get; set; }
	public abstract string Id { get; }
	public IReadOnlyList<IMutableControlFlowIncomingBranch> Incoming => _incoming;
	public IReadOnlyList<IMutableControlFlowOutgoingBranch> Outgoing => _outgoing;
	#endregion

	#region Methods
	public virtual void AddIncoming(IMutableControlFlowIncomingBranch incoming)
	{
		if (incoming is IControlFlowBidirectionalBranch branch && branch.To != this)
			ThrowHelper.ThrowArgumentException(nameof(incoming), $"Expected the incoming branch's target block ({branch.To}) to be this block ({this}).");

		_incoming.Add(incoming);
	}
	public virtual void AddOutgoing(IMutableControlFlowOutgoingBranch outgoing)
	{
		if (outgoing is IControlFlowBidirectionalBranch branch && branch.From != this)
			ThrowHelper.ThrowArgumentException(nameof(outgoing), $"Expected the outgoing branch's source block ({branch.From}) to be this block ({this}).");

		_outgoing.Add(outgoing);
	}
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"Block: {Id}";
	#endregion
}


public static class IControlFlowBlockExtensions
{
	#region Functions
	private static void Flatten(List<IMutableControlFlowBlock> target, IMutableControlFlowBlock block)
	{
		target.Add(block);

		if (block is IMutableControlFlowExpressionBlock expression)
		{
			foreach (IMutableControlFlowExpressionBlock current in expression.Blocks)
				Flatten(target, current);
		}
		else if (block is IMutableControlFlowConstructBlock construct)
		{
			foreach (IMutableControlFlowBlock current in construct.Blocks)
				Flatten(target, current);
		}
	}
	#endregion

	extension(IEnumerable<IMutableControlFlowBlock> blocks)
	{
		#region Methods
		public IReadOnlyList<IMutableControlFlowBlock> Flatten()
		{
			List<IMutableControlFlowBlock> target = [];

			foreach (IMutableControlFlowBlock current in blocks)
				Flatten(target, current);

			return target;
		}
		#endregion
	}

	extension(IMutableControlFlowBlock block)
	{
		#region Methods
		public IReadOnlyList<IMutableControlFlowBlock> Flatten()
		{
			List<IMutableControlFlowBlock> target = [];
			Flatten(target, block);

			return target;
		}
		#endregion
	}

	extension(IMutableControlFlowBlock block)
	{
		#region Properties
		public IMutableControlFlowBlock EndMarkerIfConstruct => (block as IMutableControlFlowConstructBlock)?.End ?? block;
		#endregion
	}
}
