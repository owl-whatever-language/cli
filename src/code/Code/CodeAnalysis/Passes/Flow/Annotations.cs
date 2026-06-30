namespace OwlDomain.Owl.Code.CodeAnalysis.Passes.Flow;

public sealed class FlowGraphAnnotation : CodeAnnotation
{
	#region Properties
	public override string Kind => "flow_graph";
	public IFlowGraph Graph { get; }
	#endregion

	#region Constructors
	public FlowGraphAnnotation(IFlowGraph graph)
	{
		Graph = graph;
	}
	#endregion
}

public sealed class FlowBlockAnnotation : CodeAnnotation
{
	#region Properties
	public override string Kind => "flow_block";
	public IFlowBlock Block { get; }
	#endregion

	#region Constructors
	public FlowBlockAnnotation(IFlowBlock block)
	{
		Block = block;
	}
	#endregion
}

public static class FlowGraphAnnotationExtensions
{
	extension(ICodeAnnotations annotations)
	{
		#region Methods
		public void AddFlowGraph(IFlowGraph graph)
		{
			FlowGraphAnnotation annotation = new(graph);
			annotations.Add(annotation);
		}
		public void AddFlowBlock(IFlowBlock block)
		{
			FlowBlockAnnotation annotation = new(block);
			annotations.Add(annotation);
		}

		public IFlowGraph? TryGetFlowGraph() => annotations.TryGet<FlowGraphAnnotation>()?.Graph;
		public IFlowGraph GetFlowGraph() => annotations.Get<FlowGraphAnnotation>().Graph;
		public IFlowBlock GetFlowBlock() => annotations.Get<FlowBlockAnnotation>().Block;
		#endregion
	}

	extension(IAnnotatedFunctionDeclarationStatementSyntax function)
	{
		#region Properties
		public bool AlwaysReturnsValue
		{
			get
			{
				return function.Body switch
				{
					IAnnotatedBlockFunctionBodySyntax block => block.Block.Annotations.GetFlowGraph().AlwaysReturnsValue,
					IAnnotatedShortFunctionBodySyntax => true,
					IAnnotatedOnlyTerminatedFunctionBodySyntax => false,

					_ => ThrowHelper.ThrowInvalidOperationException<bool>($"Unknown function body type '{function.Body.GetType().Name}'.")
				};
			}
		}
		#endregion
	}
}
