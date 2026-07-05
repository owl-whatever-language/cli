using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;
using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;
using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Graphs;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow;

public sealed class ControlFlowAnalyser : AnalysisPass.PerTree, IDiagnosticProvider
{
	#region Nested types
	private sealed class GraphBuilder
	{
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
