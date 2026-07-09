using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;
using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow;

public static class IControlFlowExtensions
{
	extension(IControlFlowBlock block)
	{
		#region Properties
		public bool IsStart => block is IControlFlowStartBlock;
		public bool HasIncoming => block.Incoming.Any();
		#endregion

		#region Methods
		public bool IsReachable()
		{
			if (block.IsStart)
				return true;

			if (block.HasIncoming is false)
				return false;

			HashSet<IControlFlowBlock> seen = [block];
			Queue<IControlFlowBlock> check = [];
			check.Enqueue(block);

			while (check.TryDequeue(out IControlFlowBlock? current))
			{
				if (current.IsStart)
					return true;

				foreach (IControlFlowIncomingBranch branch in current.Incoming)
				{
					if (seen.Add(branch.From))
						check.Enqueue(branch.From);
				}
			}

			return false;
		}
		public bool EndsWithReturn()
		{
			if (block is IControlFlowStatementBlock statement)
				return statement.EndsWithReturn();

			HashSet<IControlFlowBlock> seen = [block];
			Queue<IControlFlowBlock> check = [];
			check.Enqueue(block);

			while (check.TryDequeue(out IControlFlowBlock? current))
			{
				if (current.IsStart)
					return false;

				if (block is IControlFlowStatementBlock statementBlock)
					return statementBlock.EndsWithReturn();

				foreach (IControlFlowIncomingBranch branch in current.Incoming)
				{
					if (seen.Add(branch.From))
						check.Enqueue(branch.From);
				}
			}

			return false;
		}
		#endregion
	}

	extension(IControlFlowStatementBlock block)
	{
		#region Methods
		public bool EndsWithReturn()
		{
			IAnnotatedStatementSyntax? last = block.Statements.LastOrDefault();
			while (last is not null)
			{
				if (last is IAnnotatedBlockStatementSyntax blockStatement)
				{
					Debug.Fail("Block statements should be treated as constructs, so this shouldn't ever actually happen.");
					last = blockStatement.Statements.LastOrDefault();
					continue;
				}

				break;
			}

			if (last?.NodeEnum is SyntaxNodeEnum.ReturnStatement or SyntaxNodeEnum.ValueReturnStatement)
				return true;

			return false;
		}
		#endregion
	}
}
