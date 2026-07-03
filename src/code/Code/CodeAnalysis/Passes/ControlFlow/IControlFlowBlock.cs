using OwlDomain.ParsingTools.Syntax.Printing;

namespace OwlDomain.Owl.Code.CodeAnalysis.Passes.ControlFlow;

public enum ControlFlowBlockKind
{
	Start,
	Middle,
	End,
}

public interface IControlFlowBlock
{
	#region Properties
	ControlFlowBlockKind Kind { get; }
	IReadOnlyList<IControlFlowBranch> Incoming { get; }
	IReadOnlyList<IControlFlowBranch> Outgoing { get; }
	IReadOnlyList<IAnnotatedSyntaxNode> Nodes { get; }
	bool IsReachable { get; }
	bool HasReturnValue { get; }
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class ControlFlowBlock : IControlFlowBlock
{
	#region Nested types
	private sealed class Builder
	{
		#region Fields
		private readonly List<IAnnotatedSyntaxNode> _nodes = [];
		private readonly List<ControlFlowBlock> _blocks = [];
		#endregion

		#region Methods
		public List<ControlFlowBlock> Build(IReadOnlyList<IAnnotatedSyntaxNode> nodes)
		{
			foreach (IAnnotatedSyntaxNode node in nodes)
			{
#pragma warning disable IDE0010 // Add missing cases
				switch (node.NodeEnum)
				{
					case SyntaxNodeEnum.LocalFunctionDeclarationStatement:
					case SyntaxNodeEnum.OnlyTerminatedStatement:
						break;

					case SyntaxNodeEnum.ReturnStatement:
					case SyntaxNodeEnum.ValueReturnStatement:
					case SyntaxNodeEnum.ShortFunctionBody:
						_nodes.Add(node);
						PrepareForNewBlock();
						break;

					case SyntaxNodeEnum.VariableDeclarationStatement:
					case SyntaxNodeEnum.ExpressionStatement:
						_nodes.Add(node);
						break;

					default:
						ThrowHelper.ThrowInvalidOperationException($"The statement type '{node.GetType().Name}' is currently not supported by the control flow block builder.");
						return default;
				}
#pragma warning restore IDE0010 // Add missing cases
			}

			TryEndExistingBlock();

			return _blocks;
		}
		#endregion

		#region Helpers
		private void PrepareForNewBlock() => TryEndExistingBlock();
		private void TryEndExistingBlock()
		{
			if (_nodes.Count is 0)
				return;

			ControlFlowBlock block = new();
			block.Nodes.AddRange(_nodes);

			foreach (IAnnotatedSyntaxNode node in _nodes)
				node.Annotations.AddControlFlowBlock(block);

			_blocks.Add(block);
			_nodes.Clear();
		}
		#endregion
	}
	#endregion

	#region Properties
	public ControlFlowBlockKind Kind { get; }
	public List<ControlFlowBranch> Incoming { get; } = [];
	public List<ControlFlowBranch> Outgoing { get; } = [];
	public List<IAnnotatedSyntaxNode> Nodes { get; } = [];
	public bool IsReachable => Incoming.Count is not 0 && Incoming.Any(b => b.From.IsReachable);
	public bool HasReturnValue => Nodes.Any(s => s is IAnnotatedValueReturnStatementSyntax);

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyList<IControlFlowBranch> IControlFlowBlock.Incoming => Incoming;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyList<IControlFlowBranch> IControlFlowBlock.Outgoing => Outgoing;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyList<IAnnotatedSyntaxNode> IControlFlowBlock.Nodes => Nodes;
	#endregion

	#region Constructors
	public ControlFlowBlock() => Kind = ControlFlowBlockKind.Middle;
	public ControlFlowBlock(ControlFlowBlockKind kind)
	{
		if (kind is not ControlFlowBlockKind.Start and not ControlFlowBlockKind.End)
			ThrowHelper.ThrowArgumentOutOfRangeException(nameof(kind), kind, "When manually creating a control flow block, it must either be a start or an end block.");

		Kind = kind;
	}
	#endregion

	#region Functions
	public static List<ControlFlowBlock> Build(IReadOnlyList<IAnnotatedSyntaxNode> nodes)
	{
		Builder builder = new();

		return builder.Build(nodes);
	}
	#endregion

	#region Helpers
	private string GetDebugName()
	{
		return Kind switch
		{
			ControlFlowBlockKind.Start => "Start",
			ControlFlowBlockKind.End => "End",
			ControlFlowBlockKind.Middle => Nodes.Any() ? Nodes[0].GetDebugSource() : "<empty>",

			_ => ThrowHelper.ThrowInvalidOperationException<string>($"Unknown control flow block kind '{Kind}'.")
		};
	}
	private string DebuggerDisplay()
	{
		if (Kind is ControlFlowBlockKind.Start)
		{
			Debug.Assert(Outgoing.Count is 1);
			return $"Start -> {Outgoing[0].To.GetDebugName()}";
		}

		if (Kind is ControlFlowBlockKind.End)
			return $"End";

		return $"Block: {GetDebugName()}";
	}
	#endregion
}

public static class IFlowBlockExtensions
{
	extension(IControlFlowBlock block)
	{
		#region Properties
		public bool IsStart => block.Kind == ControlFlowBlockKind.Start;
		public bool IsEnd => block.Kind == ControlFlowBlockKind.End;
		#endregion
	}
}
