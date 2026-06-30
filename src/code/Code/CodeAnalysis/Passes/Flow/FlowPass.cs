namespace OwlDomain.Owl.Code.CodeAnalysis.Passes.Flow;

public class FlowPass : IAnalysisPass<AnalysisPassResult>, IDiagnosticProvider
{
	#region Nested types
	private sealed class Annotator : BaseAnnotatedVisitor
	{
		#region Methods
		public void Annotate(IAnnotatedSyntaxTree tree) => Visit(tree);

		protected override bool Visit(IAnnotatedDocumentSyntax node)
		{
			AttachGraph(node, node.Statements);
			return true;
		}
		protected override bool Visit(IAnnotatedBlockStatementSyntax node)
		{
			AttachGraph(node, node.Statements);
			return true;
		}
		#endregion

		#region Function
		protected override bool Visit(IAnnotatedLocalFunctionDeclarationStatementSyntax node)
		{
			VisitChildren(node);
			TryInheritGraph(node, node.Declaration);

			return false;
		}
		protected override bool Visit(IAnnotatedFunctionDeclarationStatementSyntax node)
		{
			VisitChildren(node);
			TryInheritGraph(node, node.Body);

			return false;
		}
		protected override bool Visit(IAnnotatedBlockFunctionBodySyntax node)
		{
			VisitChildren(node);
			TryInheritGraph(node, node.Block);

			return false;
		}
		protected override bool Visit(IAnnotatedShortFunctionBodySyntax node)
		{
			AttachGraph(node, node);
			return true;
		}
		#endregion

		#region Helpers
		private void AttachGraph(IAnnotatedSyntaxNode target, params IReadOnlyList<IAnnotatedSyntaxNode> nodes)
		{
			FlowGraph graph = FlowGraph.Build(target, nodes);
			target.Annotations.AddFlowGraph(graph);
		}
		private void TryInheritGraph(IAnnotatedSyntaxNode target, IAnnotatedSyntaxNode from)
		{
			if (from.Annotations.TryGet(out FlowGraphAnnotation? annotation) is false)
				return;

			IFlowGraph graph = annotation.Graph;
			FlowGraph newGraph = new(target, graph.Start, graph.End, graph.Blocks, graph.Branches);

			target.Annotations.AddFlowGraph(newGraph);
		}
		#endregion
	}
	#endregion

	#region Properties
	public string Kind => "flow";
	public string Name => "flow_analyser";
	#endregion

	#region Methods
	public AnalysisPassResult Run(IAnalysisContext context)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			DiagnosticBag diagnostics = [];
			ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };

			Annotator annotator = new();
			Parallel.ForEach(context.Annotated, options, annotator.Annotate);

			return new(this, performance, diagnostics);
		}
	}
	#endregion
}
