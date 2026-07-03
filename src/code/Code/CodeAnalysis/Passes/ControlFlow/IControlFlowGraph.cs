
namespace OwlDomain.Owl.Code.CodeAnalysis.Passes.ControlFlow;

public interface IControlFlowGraph
{
	#region Properties
	IAnnotatedSyntaxNode Node { get; }
	IControlFlowBlock Start { get; }
	IControlFlowBlock End { get; }
	IReadOnlyList<IControlFlowBlock> Blocks { get; }
	IReadOnlyList<IControlFlowBranch> Branches { get; }
	bool AlwaysReturnsValue { get; }
	#endregion
}

public sealed class ControlFlowGraph : IControlFlowGraph
{
	#region Nested types
	private sealed class Builder
	{
		#region Fields
		private readonly Dictionary<IAnnotatedSyntaxNode, ControlFlowBlock> _byNode = [];

		private readonly ControlFlowBlock _start = new(ControlFlowBlockKind.Start);
		private readonly ControlFlowBlock _end = new(ControlFlowBlockKind.End);
		private readonly List<ControlFlowBranch> _branches = [];
		#endregion

		#region Methods
		public ControlFlowGraph Build(IAnnotatedSyntaxNode parent, List<ControlFlowBlock> blocks)
		{
			if (blocks.Any())
				Connect(_start, blocks[0]);
			else
				Connect(_start, _end);

			foreach (ControlFlowBlock block in blocks)
			{
				foreach (IAnnotatedSyntaxNode node in block.Nodes)
				{
					_byNode.Add(node, block);
				}
			}

			for (int i = 0; i < blocks.Count; i++)
			{
				ControlFlowBlock current = blocks[i];
				ControlFlowBlock next = i == blocks.Count - 1 ? _end : blocks[i + 1];

				foreach (IAnnotatedSyntaxNode node in current.Nodes)
				{
					bool isLast = current.Nodes.Last(s => s is not IAnnotatedLocalFunctionDeclarationStatementSyntax) == node;

#pragma warning disable IDE0010 // Add missing cases
					switch (node.NodeEnum)
					{
						case SyntaxNodeEnum.LocalFunctionDeclarationStatement:
						case SyntaxNodeEnum.OnlyTerminatedStatement:
							break;

						case SyntaxNodeEnum.ReturnStatement:
						case SyntaxNodeEnum.ValueReturnStatement:
						case SyntaxNodeEnum.ShortFunctionBody:
							Connect(current, _end);
							break;

						case SyntaxNodeEnum.VariableDeclarationStatement:
						case SyntaxNodeEnum.ExpressionStatement:
							if (isLast)
								Connect(current, next);
							break;

						default:
							ThrowHelper.ThrowInvalidOperationException($"The statement type '{node.GetType().Name}' is currently not supported by the control flow graph builder.");
							return default;
					}
#pragma warning restore IDE0010 // Add missing cases
				}
			}

			blocks.Insert(0, _start);
			blocks.Add(_end);

			return new(parent, _start, _end, blocks, _branches);
		}
		#endregion

		#region Helpers
		private void Connect(ControlFlowBlock from, ControlFlowBlock to)
		{
			ControlFlowBranch branch = new(from, to);
			_branches.Add(branch);

			from.Outgoing.Add(branch);
			to.Incoming.Add(branch);
		}
		#endregion
	}
	#endregion

	#region Properties
	public IAnnotatedSyntaxNode Node { get; }
	public IControlFlowBlock Start { get; }
	public IControlFlowBlock End { get; }
	public IReadOnlyList<IControlFlowBlock> Blocks { get; }
	public IReadOnlyList<IControlFlowBranch> Branches { get; }
	public bool AlwaysReturnsValue => End.Incoming.All(b => b.From.HasReturnValue);
	#endregion

	#region Constructors
	public ControlFlowGraph(
		IAnnotatedSyntaxNode node,
		IControlFlowBlock start,
		IControlFlowBlock end,
		IReadOnlyList<IControlFlowBlock> blocks,
		IReadOnlyList<IControlFlowBranch> branches)
	{
		Node = node;
		Start = start;
		End = end;
		Blocks = blocks;
		Branches = branches;
	}
	#endregion

	#region Functions
	public static ControlFlowGraph Build(IAnnotatedSyntaxNode node, IReadOnlyList<IAnnotatedSyntaxNode> nodes)
	{
		List<ControlFlowBlock> blocks = ControlFlowBlock.Build(nodes);
		ControlFlowGraph graph = Build(node, blocks);

		return graph;
	}
	public static ControlFlowGraph Build(IAnnotatedSyntaxNode node, List<ControlFlowBlock> blocks)
	{
		Builder builder = new();

		return builder.Build(node, blocks);
	}
	#endregion
}
