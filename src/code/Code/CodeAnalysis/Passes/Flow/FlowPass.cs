namespace OwlDomain.Owl.Code.CodeAnalysis.Passes.Flow;

public class FlowPass : IAnalysisPass<AnalysisPassResult>, IDiagnosticProvider
{
	#region Nested types
	private sealed class Annotator : BaseAnnotatedVisitor
	{
		#region Properties
		private FlowPass Pass { get; }
		public DiagnosticBag Diagnostics { get; } = [];
		#endregion

		#region Constructors
		public Annotator(FlowPass pass) => Pass = pass;
		#endregion

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

			if (node.Function.Return.Type != SpecialTypes.Void)
			{
				if (node.AlwaysReturnsValue is false)
				{
					ISourceFile source = node.GetTree().Source;
					Diagnostics.Add(Pass, DiagnosticKind.Error, "missing_return", source, node.Name.Position, "Function body is missing a return.");
				}
			}

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
			DiagnosticBag[] results = new DiagnosticBag[context.Annotated.Count];
			DiagnosticBag Annotate(IAnnotatedSyntaxTree tree)
			{
				Annotator annotator = new(this);
				annotator.Annotate(tree);

				return annotator.Diagnostics;
			}
			ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };
			Parallel.ForEach(context.Annotated, options, (tree, _, index) => results[index] = Annotate(tree));

			DiagnosticBag diagnostics = [.. results.SelectMany(d => d)];

			return new(this, performance, diagnostics);
		}
	}
	#endregion
}
