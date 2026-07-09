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
	public override bool EndsWithReturn => CalculateEndsWithReturn();
	#endregion

	#region Methods
	public void Add(IAnnotatedStatementSyntax statement) => _statements.Add(statement);
	public void AddRange(IEnumerable<IAnnotatedStatementSyntax> statements) => _statements.AddRange(statements);
	private bool CalculateEndsWithReturn()
	{
		IAnnotatedStatementSyntax? last = _statements.LastOrDefault();
		while (true)
		{
			// Note(Nightowl): We merge non-branching statements, so the last statement could be a container;
			if (last is IAnnotatedBlockStatementSyntax block)
			{
				last = block.Statements.LastOrDefault();
				continue;
			}

			break;
		}

		return last is IAnnotatedReturnStatementSyntax or IAnnotatedValueReturnStatementSyntax;
	}
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"Block: {Id} | {_statements.FirstOrDefault()?.GetDebugSource() ?? "<empty>"}";
	#endregion
}
