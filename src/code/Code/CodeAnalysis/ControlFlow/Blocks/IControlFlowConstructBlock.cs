using OwlDomain.ParsingTools.Syntax.Printing;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

public interface IControlFlowConstructBlock : IControlFlowBlock
{
	#region Properties
	IAnnotatedSyntaxNode Node { get; }
	IReadOnlyList<IControlFlowBlock> Blocks { get; }
	IControlFlowMarkerBlock End { get; }
	#endregion
}

public interface IMutableControlFlowConstructBlock : IControlFlowConstructBlock, IMutableControlFlowBlock
{
	#region Properties
	new IReadOnlyList<IMutableControlFlowBlock> Blocks { get; }
	new IMutableControlFlowMarkerBlock End { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyList<IControlFlowBlock> IControlFlowConstructBlock.Blocks => Blocks;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IControlFlowMarkerBlock IControlFlowConstructBlock.End => End;
	#endregion

	#region Methods
	void Add(IMutableControlFlowBlock block);
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class ControlFlowConstructBlock : MutableControlFlowBlock, IMutableControlFlowConstructBlock
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<IMutableControlFlowBlock> _blocks = [];
	#endregion

	#region Properties
	public override string Id => $"{Node.NodeKind.WithGroup}#{BlockNumber}";
	public IAnnotatedSyntaxNode Node { get; }
	public IReadOnlyList<IMutableControlFlowBlock> Blocks => _blocks;
	public IMutableControlFlowMarkerBlock End { get; }
	#endregion

	#region Constructors
	public ControlFlowConstructBlock(IAnnotatedSyntaxNode node)
	{
		Node = node;
		End = new ControlFlowMarkerBlock(this, "end");
	}
	#endregion

	#region Methods
	public void Add(IMutableControlFlowBlock block) => _blocks.Add(block);
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"Block: {Id} | {Node.GetDebugSource()}";
	#endregion
}
