using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

public interface IControlFlowMarkerBlock : IControlFlowBlock
{
	#region Properties
	IControlFlowBlock Parent { get; }
	string Name { get; }
	#endregion
}

public interface IMutableControlFlowMarkerBlock : IControlFlowMarkerBlock, IMutableControlFlowBlock
{
	#region Properties
	new IMutableControlFlowBlock Parent { get; }
	IControlFlowBlock IControlFlowMarkerBlock.Parent => Parent;
	#endregion
}

public sealed class ControlFlowMarkerBlock : MutableControlFlowBlock, IMutableControlFlowMarkerBlock
{
	#region Properties
	public IMutableControlFlowBlock Parent { get; }
	public string Name { get; }
	public override string Id => $"{Parent.Id}_{Name}_marker";
	public override bool EndsWithReturn => Incoming.All(b => b.From.EndsWithReturn);
	#endregion

	#region Constructors
	public ControlFlowMarkerBlock(IMutableControlFlowBlock parent, string name)
	{
		Parent = parent;
		Name = name;
	}
	#endregion

	#region Methods
	public override void AddOutgoing(IMutableControlFlowOutgoingBranch outgoing)
	{
		if (Outgoing.Count > 0)
		{
			// Note(Nightowl): This might change if markers are ever used for anything but temporary end blocks;
			ThrowHelper.ThrowInvalidOperationException("A control flow marker should only ever have one outgoing branch.");
		}

		base.AddOutgoing(outgoing);
	}
	#endregion
}
