using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;
using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;
using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Graphs;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow;

public sealed class ControlFlowAnalyser : AnalysisPass.PerTree, IDiagnosticProvider
{
	#region Nested types
	private sealed class GraphBuilder
	{
		#region Fields
		private readonly IMutableControlFlowGraph _graph;
		#endregion

		#region Constructors
		private GraphBuilder(IMutableControlFlowGraph graph) => _graph = graph;
		#endregion

		#region Populate methods
		private void Populate(params IReadOnlyList<IAnnotatedStatementSyntax> statements)
		{
			IReadOnlyList<IMutableControlFlowBlock> blocks = Create(statements);

			if (blocks.Any())
			{
				Connect(_graph.Start, blocks.First());
				IMutableControlFlowBlock last = blocks.Last().EndMarkerIfConstruct;

				if (ConnectLastBlock(last))
					Connect(last, _graph.End);
			}
			else
				Connect(_graph.Start, _graph.End);

			IReadOnlyList<IMutableControlFlowBlock> all = blocks.Flatten();
			_graph.AddRange(all);
		}
		public void Populate(IAnnotatedExpressionSyntax expression)
		{
			IMutableControlFlowExpressionBlock block = Create(expression, _graph.End);
			Connect(_graph.Start, block);

			IReadOnlyList<IMutableControlFlowBlock> all = block.Flatten();
			_graph.AddRange(all);
		}
		private bool ConnectLastBlock(IMutableControlFlowBlock last)
		{
			if (last is not IControlFlowStatementBlock block)
				return true;

			IAnnotatedStatementSyntax? statement = block.Statements.LastOrDefault();
			if (statement is null)
				return true;

			if (statement is IAnnotatedReturnStatementSyntax or IAnnotatedValueReturnStatementSyntax)
				return false;

			return true;
		}
		#endregion

		#region Block methods
		private List<IMutableControlFlowBlock> Create(params IReadOnlyList<IAnnotatedStatementSyntax> statements)
		{
			List<IMutableControlFlowBlock> blocks = [];
			ControlFlowStatementBlock block = new();

			void EndBlock()
			{
				if (block.Statements.Count is 0)
				{
					Debug.Assert(block.Incoming.Count is 0);
					Debug.Assert(block.Outgoing.Count is 0);

					return;
				}

				blocks.Add(block);
				block = new();
			}

			void IfBranching(IAnnotatedStatementSyntax statement, IAnnotatedExpressionSyntax expression)
			{
				if (WillBranch(expression))
				{
					EndBlock();

					IMutableControlFlowExpressionBlock value = Create(expression, block);
					blocks.Add(value);
				}

				block.Add(statement);
			}

			foreach (IAnnotatedStatementSyntax statement in statements)
			{
				if (statement.IsExecutable is false)
					continue;

				if (TryCreateConstruct(statement, out IMutableControlFlowBlock? construct))
				{
					EndBlock();
					blocks.Add(construct);
				}
				else if (statement is IAnnotatedReturnStatementSyntax)
				{
					block.Add(statement);
					Connect(block, _graph.End);

					EndBlock();
				}
				else if (statement is IAnnotatedValueReturnStatementSyntax @return)
				{
					IfBranching(@return, @return.Value);
					Connect(block, _graph.End);

					EndBlock();
				}
				else if (statement is IAnnotatedExpressionStatementSyntax expr)
					IfBranching(expr, expr.Expression);
				else if (statement is IAnnotatedVariableDeclarationStatementSyntax variable)
					IfBranching(variable, variable.Value);
				else
					ThrowHelper.ThrowInvalidOperationException($"Unhandled executable statement type ({statement.GetType().Name}).");
			}

			EndBlock();

			for (int i = 0; i < blocks.Count - 1; i++)
			{
				IMutableControlFlowBlock current = blocks[i];
				IMutableControlFlowBlock next = blocks[i + 1];

				if (current is IControlFlowExpressionBlock)
				{
					// Note(Nightowl):
					// We only want to do block -> expression, never expression -> block
					// as that will all be handled by the previous block building.
					continue;
				}

				Connect(current.EndMarkerIfConstruct, next);
			}

			return blocks;
		}
		#endregion

		#region Functions
		public static void Populate(IMutableControlFlowGraph graph, params IReadOnlyList<IAnnotatedStatementSyntax> statements)
		{
			GraphBuilder builder = new(graph);
			builder.Populate(statements);
		}
		public static void Populate(IMutableControlFlowGraph graph, IAnnotatedExpressionSyntax expression)
		{
			GraphBuilder builder = new(graph);
			builder.Populate(expression);
		}
		#endregion

		#region Construct methods
		private bool TryCreateConstruct(IAnnotatedStatementSyntax statement, [NotNullWhen(true)] out IMutableControlFlowBlock? construct)
		{
			construct = statement switch
			{
				IAnnotatedBlockStatementSyntax block => Create(block),
				IAnnotatedIfStatementSyntax @if => Create(@if),
				IAnnotatedIfElseStatementSyntax ifElse => Create(ifElse),
				IAnnotatedWhileStatementSyntax @while => Create(@while),

				_ => null,
			};

			return construct is not null;
		}
		private IMutableControlFlowBlock Create(IAnnotatedBlockStatementSyntax statement)
		{
			// Note(Nightowl): Not strictly a control flow construct but it's easier than dealing with inlining the statements;
			ControlFlowConstructBlock construct = new("block", statement);
			IReadOnlyList<IMutableControlFlowBlock> body = Create(statement.Statements);
			construct.AddRange(body);

			if (body.Any())
			{
				Connect(construct, body.First());
				Connect(body.Last().EndMarkerIfConstruct, construct.End);
			}
			else
				Connect(construct, construct.End);

			return construct;
		}
		private IMutableControlFlowBlock Create(IAnnotatedIfStatementSyntax statement)
		{
			ControlFlowConstructBlock construct = new("if", statement);
			IReadOnlyList<IMutableControlFlowBlock> body = Create(statement.TrueClause);
			construct.AddRange(body);

			IMutableControlFlowBlock target = body.FirstOrDefault(construct.End); // In-case body is empty
			IMutableControlFlowExpressionBlock condition = Create("if", statement.Condition, target, construct.End);
			construct.Add(condition);

			Connect(construct, condition);
			return construct;
		}
		private IMutableControlFlowBlock Create(IAnnotatedIfElseStatementSyntax statement)
		{
			ControlFlowConstructBlock construct = new("if", statement);
			IReadOnlyList<IMutableControlFlowBlock> trueBody = Create(statement.TrueClause);
			IReadOnlyList<IMutableControlFlowBlock> falseBody = Create(statement.FalseClause);

			construct.AddRange(trueBody);
			construct.AddRange(falseBody);

			if (trueBody.Any())
				Connect(trueBody.Last().EndMarkerIfConstruct, construct.End);

			if (falseBody.Any())
				Connect(falseBody.Last().EndMarkerIfConstruct, construct.End);

			IMutableControlFlowBlock trueTarget = trueBody.FirstOrDefault(construct.End); // In-case body is empty
			IMutableControlFlowBlock falseTarget = falseBody.FirstOrDefault(construct.End); // In-case body is empty

			IMutableControlFlowExpressionBlock condition = Create("if", statement.Condition, trueTarget, falseTarget);
			construct.Add(condition);

			Connect(construct, condition);
			return construct;
		}
		private IMutableControlFlowBlock Create(IAnnotatedWhileStatementSyntax statement)
		{
			ControlFlowConstructBlock construct = new("while", statement);
			List<IMutableControlFlowBlock> body = Create(statement.Body);

			if (body.Count is 0)
			{
				ControlFlowMarkerBlock emptyBody = new(construct, "body");
				body.Add(emptyBody);
			}
			construct.AddRange(body);

			IMutableControlFlowExpressionBlock condition = Create("while", statement.Condition, body.First(), construct.End);
			construct.Add(condition);

			Connect(construct, condition);
			Connect(body.Last().EndMarkerIfConstruct, condition);

			return construct;
		}
		#endregion

		#region Expression methods
		private IMutableControlFlowExpressionBlock Create(
			string constructName,
			IAnnotatedExpressionSyntax condition,
			IMutableControlFlowBlock success,
			IMutableControlFlowBlock failure)
		{
			ControlFlowExpressionBlock block = new(condition, constructName);

			Connect(block, success, condition);
			ConnectNegated(block, failure, condition);

			return block;
		}
		private IMutableControlFlowExpressionBlock Create(IAnnotatedExpressionSyntax expression, IMutableControlFlowBlock end)
		{
			ControlFlowExpressionBlock block = new(expression);

			Connect(block, end);

			return block;
		}
		#endregion

		#region Will branch methods
		private static bool WillBranch(IAnnotatedExpressionSyntax expression)
		{
			return expression switch
			{
				IAnnotatedGetExpressionSyntax => false,
				IAnnotatedBinaryExpressionSyntax binary => WillBranch(binary),

				_ => expression.Flatten<IAnnotatedExpressionSyntax>().Skip(1).Any(WillBranch),
			};
		}
		private static bool WillBranch(IAnnotatedBinaryExpressionSyntax expression)
		{
			SyntaxKind op = expression.Operator.Kind;

			if (op == SyntaxKind.DoublePipe || op == SyntaxKind.DoubleAmpersand)
				return true;

			return WillBranch(expression.Left) || WillBranch(expression.Right);
		}
		#endregion

		#region Connect methods
		private void Connect(IMutableControlFlowBlock from, IMutableControlFlowBlock to)
		{
			UnconditionalControlFlowBranch branch = new(from, to);
			_graph.Add(branch);

			from.AddOutgoing(branch);
			to.AddIncoming(branch);
		}
		private void Connect(IMutableControlFlowBlock from, IMutableControlFlowBlock to, IAnnotatedExpressionSyntax condition)
		{
			ConditionalControlFlowBranch branch = new(condition, isNegated: false, from, to);
			_graph.Add(branch);

			from.AddOutgoing(branch);
			to.AddIncoming(branch);
		}
		private void ConnectNegated(IMutableControlFlowBlock from, IMutableControlFlowBlock to, IAnnotatedExpressionSyntax condition)
		{
			ConditionalControlFlowBranch branch = new(condition, isNegated: true, from, to);
			_graph.Add(branch);

			from.AddOutgoing(branch);
			to.AddIncoming(branch);
		}
		#endregion
	}
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

			if (graph.Node is IAnnotatedDocumentSyntax document)
				GraphBuilder.Populate(graph, document.Statements);
			else if (graph.Node is IAnnotatedFunctionDeclarationStatementSyntax function)
			{
				if (function.Body is IAnnotatedBlockFunctionBodySyntax block)
					GraphBuilder.Populate(graph, block.Block.Statements);
				else if (function.Body is IAnnotatedShortFunctionBodySyntax @short)
					GraphBuilder.Populate(graph, @short.Expression);
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
