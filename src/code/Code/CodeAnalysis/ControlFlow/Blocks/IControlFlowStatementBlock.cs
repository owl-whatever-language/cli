using OwlDomain.ParsingTools.Syntax.Printing;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;

public interface IControlFlowStatementBlock : IControlFlowBlock
{
	#region Properties
	IReadOnlyList<IAnnotatedStatementSyntax> Statements { get; }
	#endregion
}

public interface IMutableControlFlowStatementBlock : IControlFlowStatementBlock
{
	#region Methods
	void Add(IAnnotatedStatementSyntax statement);
	void AddRange(IEnumerable<IAnnotatedStatementSyntax> statements);
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class ControlFlowStatementBlock : MutableControlFlowBlock, IMutableControlFlowStatementBlock
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<IAnnotatedStatementSyntax> _statements = [];
	#endregion

	#region Properties
	public override string Id => (_statements.FirstOrDefault()?.NodeKind.WithGroup ?? "block") + $"#{BlockNumber}";
	public IReadOnlyList<IAnnotatedStatementSyntax> Statements => _statements;
	#endregion

	#region Methods
	public void Add(IAnnotatedStatementSyntax statement) => _statements.Add(statement);
	public void AddRange(IEnumerable<IAnnotatedStatementSyntax> statements) => _statements.AddRange(statements);
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"Block: {Id} | {_statements.FirstOrDefault()?.GetDebugSource() ?? "<empty>"}";
	#endregion
}
