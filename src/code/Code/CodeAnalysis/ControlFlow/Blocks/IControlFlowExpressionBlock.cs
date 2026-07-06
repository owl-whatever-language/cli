using OwlDomain.ParsingTools.Syntax.Printing;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

public interface IControlFlowExpressionBlock : IControlFlowBlock
{
	#region Properties
	string? ConstructName { get; }
	IAnnotatedExpressionSyntax Expression { get; }
	IReadOnlyList<IControlFlowExpressionBlock> Blocks { get; }
	#endregion
}

public interface IMutableControlFlowExpressionBlock : IControlFlowExpressionBlock, IMutableControlFlowBlock
{
	#region Properties
	new IReadOnlyList<IMutableControlFlowExpressionBlock> Blocks { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyList<IControlFlowExpressionBlock> IControlFlowExpressionBlock.Blocks => Blocks;
	#endregion

	#region Methods
	void Add(IMutableControlFlowExpressionBlock expression);
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class ControlFlowExpressionBlock : MutableControlFlowBlock, IMutableControlFlowExpressionBlock
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<IMutableControlFlowExpressionBlock> _blocks = [];
	#endregion

	#region Properties
	public override string Id => $"{Expression.NodeKind.WithGroup}#{BlockNumber}";
	public string? ConstructName { get; }
	public IAnnotatedExpressionSyntax Expression { get; }
	public IReadOnlyList<IMutableControlFlowExpressionBlock> Blocks => _blocks;
	#endregion

	#region Constructors
	public ControlFlowExpressionBlock(IAnnotatedExpressionSyntax expression, string? constructName = null)
	{
		Expression = expression;
		ConstructName = constructName;
	}
	#endregion

	#region Methods
	public void Add(IMutableControlFlowExpressionBlock expression) => _blocks.Add(expression);
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"Block: {Id} | {Expression.GetDebugSource()}";
	#endregion
}
