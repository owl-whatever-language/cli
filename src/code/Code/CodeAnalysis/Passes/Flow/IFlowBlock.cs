using OwlDomain.ParsingTools.Syntax.Printing;

namespace OwlDomain.Owl.Code.CodeAnalysis.Passes.Flow;

public enum FlowBlockKind
{
	Start,
	Middle,
	End,
}

public interface IFlowBlock
{
	#region Properties
	FlowBlockKind Kind { get; }
	IReadOnlyList<IFlowBranch> Incoming { get; }
	IReadOnlyList<IFlowBranch> Outgoing { get; }
	IReadOnlyList<IAnnotatedStatementSyntax> Statements { get; }
	bool IsReachable { get; }
	bool HasReturnValue { get; }
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class FlowBlock : IFlowBlock
{
	#region Nested types
	private sealed class Builder
	{
		#region Fields
		private readonly List<IAnnotatedStatementSyntax> _statements = [];
		private readonly List<FlowBlock> _blocks = [];
		#endregion

		#region Methods
		public List<FlowBlock> Build(IReadOnlyList<IAnnotatedStatementSyntax> statements)
		{
			foreach (IAnnotatedStatementSyntax statement in statements)
			{
#pragma warning disable IDE0010 // Add missing cases
				switch (statement.NodeEnum)
				{
					case SyntaxNodeEnum.LocalFunctionDeclarationStatement:
					case SyntaxNodeEnum.OnlyTerminatedStatement:
						break;

					case SyntaxNodeEnum.ReturnStatement:
					case SyntaxNodeEnum.ValueReturnStatement:
						_statements.Add(statement);
						PrepareForNewBlock();
						break;

					case SyntaxNodeEnum.VariableDeclarationStatement:
					case SyntaxNodeEnum.ExpressionStatement:
						_statements.Add(statement);
						break;

					default:
						ThrowHelper.ThrowInvalidOperationException($"The statement type '{statement.GetType().Name}' is currently not supported by the flow block builder.");
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
			if (_statements.Count is 0)
				return;

			FlowBlock block = new();
			block.Statements.AddRange(_statements);

			foreach (IAnnotatedStatementSyntax statement in _statements)
				statement.Annotations.AddFlowBlock(block);

			_blocks.Add(block);
			_statements.Clear();
		}
		#endregion
	}
	#endregion

	#region Properties
	public FlowBlockKind Kind { get; }
	public List<FlowBranch> Incoming { get; } = [];
	public List<FlowBranch> Outgoing { get; } = [];
	public List<IAnnotatedStatementSyntax> Statements { get; } = [];
	public bool IsReachable => Incoming.Count is not 0 && Incoming.Any(b => b.From.IsReachable);
	public bool HasReturnValue => Statements.Any(s => s is IAnnotatedValueReturnStatementSyntax);

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyList<IFlowBranch> IFlowBlock.Incoming => Incoming;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyList<IFlowBranch> IFlowBlock.Outgoing => Outgoing;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyList<IAnnotatedStatementSyntax> IFlowBlock.Statements => Statements;
	#endregion

	#region Constructors
	public FlowBlock() => Kind = FlowBlockKind.Middle;
	public FlowBlock(FlowBlockKind kind)
	{
		if (kind is not FlowBlockKind.Start and not FlowBlockKind.End)
			ThrowHelper.ThrowArgumentOutOfRangeException(nameof(kind), kind, "When manually creating a flow block, it must either be a start or an end block.");

		Kind = kind;
	}
	#endregion

	#region Functions
	public static List<FlowBlock> Build(IReadOnlyList<IAnnotatedStatementSyntax> statements)
	{
		Builder builder = new();

		return builder.Build(statements);
	}
	#endregion


	#region Helpers
	private string GetDebugName()
	{
		return Kind switch
		{
			FlowBlockKind.Start => "Start",
			FlowBlockKind.End => "End",
			FlowBlockKind.Middle => Statements.Any() ? Statements[0].GetDebugSource() : "<empty>",

			_ => ThrowHelper.ThrowInvalidOperationException<string>($"Unknown flow block kind '{Kind}'.")
		};
	}
	private string DebuggerDisplay()
	{
		if (Kind is FlowBlockKind.Start)
		{
			Debug.Assert(Outgoing.Count is 1);
			return $"Start -> {Outgoing[0].To.GetDebugName()}";
		}

		if (Kind is FlowBlockKind.End)
			return $"End";

		return $"Block: {GetDebugName()}";
	}
	#endregion
}

public static class IFlowBlockExtensions
{
	extension(IFlowBlock block)
	{
		#region Properties
		public bool IsStart => block.Kind == FlowBlockKind.Start;
		public bool IsEnd => block.Kind == FlowBlockKind.End;
		#endregion
	}
}
