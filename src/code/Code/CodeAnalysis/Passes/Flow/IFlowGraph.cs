
namespace OwlDomain.Owl.Code.CodeAnalysis.Passes.Flow;

public interface IFlowGraph
{
	#region Properties
	IFlowBlock Start { get; }
	IFlowBlock End { get; }
	IReadOnlyList<IFlowBlock> Blocks { get; }
	IReadOnlyList<IFlowBranch> Branches { get; }
	bool AlwaysReturnsValue { get; }
	#endregion
}

public sealed class FlowGraph : IFlowGraph
{
	#region Nested types
	private sealed class Builder
	{
		#region Fields
		private readonly Dictionary<IAnnotatedStatementSyntax, FlowBlock> _byStatement = [];

		private readonly FlowBlock _start = new(FlowBlockKind.Start);
		private readonly FlowBlock _end = new(FlowBlockKind.End);
		private readonly List<FlowBranch> _branches = [];
		#endregion

		#region Methods
		public FlowGraph Build(List<FlowBlock> blocks)
		{
			if (blocks.Any())
				Connect(_start, blocks[0]);
			else
				Connect(_start, _end);

			foreach (FlowBlock block in blocks)
			{
				foreach (IAnnotatedStatementSyntax statement in block.Statements)
				{
					_byStatement.Add(statement, block);
				}
			}

			for (int i = 0; i < blocks.Count; i++)
			{
				FlowBlock current = blocks[i];
				FlowBlock next = i == blocks.Count - 1 ? _end : blocks[i + 1];

				foreach (IAnnotatedStatementSyntax statement in current.Statements)
				{
					bool isLast = current.Statements.Last(s => s is not IAnnotatedLocalFunctionDeclarationStatementSyntax) == statement;

#pragma warning disable IDE0010 // Add missing cases
					switch (statement.NodeEnum)
					{
						case SyntaxNodeEnum.LocalFunctionDeclarationStatement:
						case SyntaxNodeEnum.OnlyTerminatedStatement:
							break;

						case SyntaxNodeEnum.ReturnStatement:
						case SyntaxNodeEnum.ValueReturnStatement:
							Connect(current, _end);
							break;

						case SyntaxNodeEnum.VariableDeclarationStatement:
						case SyntaxNodeEnum.ExpressionStatement:
							if (isLast)
								Connect(current, next);
							break;

						default:
							ThrowHelper.ThrowInvalidOperationException($"The statement type '{statement.GetType().Name}' is currently not supported by the flow graph builder.");
							return default;
					}
#pragma warning restore IDE0010 // Add missing cases
				}
			}

			blocks.Insert(0, _start);
			blocks.Add(_end);

			return new(_start, _end, blocks, _branches);
		}
		#endregion

		#region Helpers
		private void Connect(FlowBlock from, FlowBlock to)
		{
			FlowBranch branch = new(from, to);
			_branches.Add(branch);

			from.Outgoing.Add(branch);
			to.Incoming.Add(branch);
		}
		#endregion
	}
	#endregion

	#region Properties
	public IFlowBlock Start { get; }
	public IFlowBlock End { get; }
	public IReadOnlyList<IFlowBlock> Blocks { get; }
	public IReadOnlyList<IFlowBranch> Branches { get; }
	public bool AlwaysReturnsValue => End.Incoming.All(b => b.From.HasReturnValue);
	#endregion

	#region Constructors
	public FlowGraph(
		IFlowBlock start,
		IFlowBlock end,
		IReadOnlyList<IFlowBlock> blocks,
		IReadOnlyList<IFlowBranch> branches)
	{
		Start = start;
		End = end;
		Blocks = blocks;
		Branches = branches;
	}
	#endregion

	#region Functions
	public static FlowGraph Build(IReadOnlyList<IAnnotatedStatementSyntax> statements)
	{
		List<FlowBlock> blocks = FlowBlock.Build(statements);
		FlowGraph graph = Build(blocks);

		return graph;
	}
	public static FlowGraph Build(List<FlowBlock> blocks)
	{
		Builder builder = new();

		return builder.Build(blocks);
	}
	#endregion
}
