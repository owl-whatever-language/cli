using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;
using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;
using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Graphs;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow;

public sealed class ControlFlowAnalyser : AnalysisPass.PerTree, IDiagnosticProvider
{
	#region Nested types
	private sealed class Instance : BaseAnnotatedVisitor
	{
		#region Properties
		private ControlFlowAnalyser Analyser { get; }
		public DiagnosticBag Diagnostics { get; } = [];
		private ISourceFile Source { get; }
		#endregion

		#region Constructors
		public Instance(ControlFlowAnalyser analyser, ISourceFile source)
		{
			Analyser = analyser;
			Source = source;
		}
		#endregion

		#region Methods
		protected override bool Visit(IAnnotatedDocumentSyntax node)
		{
			DocumentControlFlowGraph graph = new(node);
			PopulateGraph(graph);

			return true;
		}

		protected override bool Visit(IAnnotatedFunctionDeclarationStatementSyntax node)
		{
			FunctionControlFlowGraph graph = new(node);
			PopulateGraph(graph);

			if (node.Function.Return.Type == SpecialTypes.Void)
				return true;

			if (node.Body is not IAnnotatedBlockFunctionBodySyntax)
				return true; // These will have errors elsewhere so don't duplicate.

			IReadOnlyCollection<IControlFlowBlock> missingReturns = GetBlocksWithMissingReturn(graph.End);
			foreach (IControlFlowBlock block in missingReturns)
			{
				IndexedPositionRange position = GetBestMissingReturnErrorPosition(graph, block);
				AddError("missing_return_statement", position, "The function is missing a return statement.");
			}

			return true;
		}
		private IndexedPositionRange GetBestMissingReturnErrorPosition(IControlFlowGraph graph, IControlFlowBlock block)
		{
			// Note(Nightowl): Improve this later;

			return block switch
			{
				IControlFlowStatementBlock statement => statement.Statements.Any() ? statement.Statements[^1].Position : graph.Node.Position,
				IControlFlowExpressionBlock expression => expression.Expression.Position,

				_ => ThrowHelper.ThrowInvalidOperationException<IndexedPositionRange>($"Unsupported control flow block type ({block.GetType().Name}).")
			};
		}
		private IReadOnlyCollection<IControlFlowBlock> GetBlocksWithMissingReturn(IControlFlowEndBlock end)
		{
			List<IControlFlowBlock> missing = [];

			foreach (IControlFlowIncomingBranch branch in end.Incoming)
			{
				// Note(Nightowl): 
				// We only call this function for block body functions so there won't
				// be any expression blocks that directly connect to the end block;
				IControlFlowStatementBlock from = (IControlFlowStatementBlock)branch.From;
				IAnnotatedStatementSyntax? last = from.Statements.LastOrDefault();
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

				// Note(Nightowl): Return statements that don't have a value will have an error during semantic resolution;
				if (last is IAnnotatedReturnStatementSyntax or IAnnotatedValueReturnStatementSyntax)
					continue;

				missing.Add(from);
			}

			return missing;
		}
		#endregion

		#region Graph methods
		private void PopulateGraph(IMutableControlFlowGraph graph)
		{
			ControlFlowGraphAnnotation annotation = new(graph);
			graph.Node.Annotations.Add(annotation);

			IReadOnlyList<IMutableControlFlowBlock> blocks = GetBlocks(graph.Node);
			ConnectBlocks(graph, blocks);
		}
		#endregion

		#region Block generation methods
		private IReadOnlyList<IMutableControlFlowBlock> GetBlocks(IAnnotatedSyntaxNode node)
		{
			return node switch
			{
				IAnnotatedDocumentSyntax document => GetBlocks(document),
				IAnnotatedFunctionDeclarationStatementSyntax function => GetBlocks(function),

				_ => ThrowHelper.ThrowInvalidOperationException<IReadOnlyList<IMutableControlFlowBlock>>($"Unsupported syntax node type ({node.GetType().Name}).")
			};
		}
		private IReadOnlyList<IMutableControlFlowBlock> GetBlocks(IAnnotatedDocumentSyntax node)
		{
			return GetStatementBlocks(node.Statements);
		}
		private IReadOnlyList<IMutableControlFlowBlock> GetBlocks(IAnnotatedFunctionDeclarationStatementSyntax node)
		{
			return node.Body switch
			{
				IAnnotatedShortFunctionBodySyntax body => GetBlocks(body),
				IAnnotatedBlockFunctionBodySyntax body => GetBlocks(body),
				IAnnotatedOnlyTerminatedFunctionBodySyntax => [],
				IAnnotatedEmptyFunctionBodySyntax => [],

				_ => ThrowHelper.ThrowInvalidOperationException<IReadOnlyList<IMutableControlFlowBlock>>($"Unsupported function body type ({node.GetType().Name}).")
			};
		}
		private IReadOnlyList<IMutableControlFlowBlock> GetBlocks(IAnnotatedShortFunctionBodySyntax node)
		{
			ControlFlowExpressionBlock entry = GetExpressionBlocks(node.Expression);

			return [entry];
		}
		private IReadOnlyList<IMutableControlFlowBlock> GetBlocks(IAnnotatedBlockFunctionBodySyntax node)
		{
			return GetStatementBlocks(node.Block);
		}
		private IReadOnlyList<IMutableControlFlowBlock> GetStatementBlocks(params IReadOnlyList<IAnnotatedStatementSyntax> statements)
		{
			List<IMutableControlFlowBlock> blocks = [];
			ControlFlowStatementBlock current = new();

			#region Helper functions	
			void EndLastBlock()
			{
				if (current.Statements.Count is 0)
					return;

				blocks.Add(current);
				current = new();
			}
			#endregion

			foreach (IAnnotatedStatementSyntax statement in statements)
			{
#pragma warning disable IDE0010 // Add missing cases
				switch (statement.NodeEnum)
				{
					// Note(Nightowl): Declarations that are position independent will not affect control flow;
					case SyntaxNodeEnum.LocalFunctionDeclarationStatement:
					case SyntaxNodeEnum.FunctionDeclarationStatement:
						continue;

					case SyntaxNodeEnum.OnlyTerminatedStatement:
					case SyntaxNodeEnum.EmptyStatement:
						continue;

					default:
						break;
				}
#pragma warning restore IDE0010 // Add missing cases

				if (statement is IAnnotatedBlockStatementSyntax block && WillBranch(block))
				{
					EndLastBlock();
					IReadOnlyList<IMutableControlFlowBlock> subBlocks = GetStatementBlocks(block.Statements);
					blocks.AddRange(subBlocks);

					// Note(Nightowl): Specifically do not add the block statement;
					continue;
				}

#pragma warning disable IDE0010 // Add missing cases
				switch (statement.NodeEnum)
				{
					case SyntaxNodeEnum.BlockStatement:
					case SyntaxNodeEnum.ExpressionStatement:
					case SyntaxNodeEnum.VariableDeclarationStatement:
						current.Add(statement);
						continue;

					case SyntaxNodeEnum.ReturnStatement:
					case SyntaxNodeEnum.ValueReturnStatement:
						current.Add(statement);
						EndLastBlock();
						continue;

					default:
						ThrowHelper.ThrowInvalidOperationException($"Unhandled statement type ({statement.GetType().Name}).");
						return default;
				}
#pragma warning restore IDE0010 // Add missing cases
			}

			EndLastBlock();

			return blocks;
		}

		private ControlFlowExpressionBlock GetExpressionBlocks(IAnnotatedExpressionSyntax expression)
		{
			if (WillBranch(expression))
				return GetBranchedExpressionBlocks(expression);

			ControlFlowExpressionBlock block = new(expression);
			return block;
		}
		private ControlFlowExpressionBlock GetBranchedExpressionBlocks(IAnnotatedExpressionSyntax expression)
		{
			throw new NotImplementedException($"Branching expression control flow has not been implemented yet, on account of it not existing as a language feature just yet.");
		}
		#endregion

		#region Will branch methods
		private bool WillBranch(IAnnotatedSyntaxNode node)
		{
			return node switch
			{
				IAnnotatedStatementSyntax statement => WillBranch(statement),

				_ => false,
			};
		}
		private bool WillBranch(IAnnotatedStatementSyntax node)
		{
			return node switch
			{
				IAnnotatedBlockStatementSyntax block => block.Statements.Any(WillBranch),
				IAnnotatedExpressionStatementSyntax statement => WillBranch(statement.Expression),
				IAnnotatedVariableDeclarationStatementSyntax statement => WillBranch(statement.Value),
				IAnnotatedValueReturnStatementSyntax statement => WillBranch(statement.Value),

				_ => false,
			};
		}
		private bool WillBranch(IAnnotatedExpressionSyntax node)
		{
			return node switch
			{
				_ => false,
			};
		}
		#endregion

		#region Block connection methods
		private void ConnectBlocks(IMutableControlFlowGraph graph, IReadOnlyList<IMutableControlFlowBlock> blocks)
		{
			static void Connect(IMutableControlFlowBlock from, IMutableControlFlowBlock to)
			{
				UnconditionalControlFlowBranch branch = new(from, to);
				from.AddOutgoing(branch);
				to.AddIncoming(branch);
			}

			if (blocks.Count is 0)
			{
				Connect(graph.Start, graph.End);
				return;
			}

			Connect(graph.Start, blocks[0]);

			for (int i = 0; i < blocks.Count; i++)
			{
				IMutableControlFlowBlock currentUntyped = blocks[i];
				IMutableControlFlowBlock next = i + 1 == blocks.Count ? graph.End : blocks[i];

				if (currentUntyped is IControlFlowExpressionBlock)
				{
					Debug.Assert(next is not IControlFlowExpressionBlock);
					Connect(currentUntyped, next);

					continue;
				}

				ControlFlowStatementBlock current = (ControlFlowStatementBlock)currentUntyped;
				foreach (IAnnotatedStatementSyntax statement in current.Statements)
				{
					bool isLast = statement == current.Statements[^1];

#pragma warning disable IDE0010 // Add missing cases
					switch (statement.NodeEnum)
					{
						case SyntaxNodeEnum.ValueReturnStatement:
						case SyntaxNodeEnum.ReturnStatement:
							Connect(current, next);
							break;

						case SyntaxNodeEnum.BlockStatement:
						case SyntaxNodeEnum.VariableDeclarationStatement:
						case SyntaxNodeEnum.ExpressionStatement:
							if (isLast)
								Connect(current, next);
							break;

						default:
							ThrowHelper.ThrowInvalidOperationException($"Unhandled statement type ({statement.GetType().Name}).");
							break;
					}
#pragma warning restore IDE0010 // Add missing cases
				}
			}
		}
		#endregion

		#region Helpers
		private void AddError(string id, IndexedPositionRange position, string message, StackTrace? stackTrace = null)
		{
			AddDiagnostic(DiagnosticKind.Error, id, position, message, stackTrace);
		}
		private void AddDiagnostic(DiagnosticKind kind, string id, IndexedPositionRange position, string message, StackTrace? stackTrace = null)
		{
			Diagnostics.Add(Analyser, kind, id, Source, position, message, stackTrace);
		}
		#endregion
	}
	#endregion

	#region Properties
	public string Name => "control_flow_analyser";
	public override string Kind => "control_flow";
	#endregion

	#region Methods
	protected override IDiagnosticBag Run(IAnalysisContext context, IAnnotatedSyntaxTree tree)
	{
		Instance instance = new(this, tree.Source);
		instance.Visit(tree);

		return instance.Diagnostics;
	}
	#endregion
}
