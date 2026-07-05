using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Graphs;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

public interface IControlFlowStatementBlock : IControlFlowBlock
{
	#region Properties
	IReadOnlyList<IAnnotatedStatementSyntax> Statements { get; }
	#endregion
}

public sealed class ControlFlowStatementBlock : MutableControlFlowBlock, IControlFlowStatementBlock
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<IAnnotatedStatementSyntax> _statements = [];
	#endregion

	#region Properties
	public int BlockNumber { get; set; }
	public override string Id => (_statements.FirstOrDefault()?.NodeKind.WithGroup ?? "block") + $"#{BlockNumber}";
	public IReadOnlyList<IAnnotatedStatementSyntax> Statements => _statements;
	#endregion

	#region Constructors
	public ControlFlowStatementBlock(IMutableControlFlowGraph graph)
	{
		graph.Add(this);
	}
	#endregion

	#region Methods
	public void Add(IAnnotatedStatementSyntax statement) => _statements.Add(statement);
	#endregion
}
