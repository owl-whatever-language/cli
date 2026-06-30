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
			FlowGraph graph = FlowGraph.Build(node.Statements);
			node.Annotations.AddFlowGraph(graph);

			return true;
		}
		protected override bool Visit(IAnnotatedBlockStatementSyntax node)
		{
			FlowGraph graph = FlowGraph.Build(node.Statements);
			node.Annotations.AddFlowGraph(graph);

			return true;
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
