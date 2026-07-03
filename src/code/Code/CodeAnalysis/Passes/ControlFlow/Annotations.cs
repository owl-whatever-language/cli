namespace OwlDomain.Owl.Code.CodeAnalysis.Passes.ControlFlow;

public sealed class ControlFlowGraphAnnotation : CodeAnnotation
{
	#region Properties
	public override string Kind => "control_flow_graph";
	public IControlFlowGraph Graph { get; }
	#endregion

	#region Constructors
	public ControlFlowGraphAnnotation(IControlFlowGraph graph)
	{
		Graph = graph;
	}
	#endregion
}

public sealed class ControlFlowBlockAnnotation : CodeAnnotation
{
	#region Properties
	public override string Kind => "control_flow_block";
	public IControlFlowBlock Block { get; }
	#endregion

	#region Constructors
	public ControlFlowBlockAnnotation(IControlFlowBlock block)
	{
		Block = block;
	}
	#endregion
}

public static class ControlFlowGraphAnnotationExtensions
{
	extension(ICodeAnnotations annotations)
	{
		#region Methods
		public void AddControlFlowGraph(IControlFlowGraph graph)
		{
			ControlFlowGraphAnnotation annotation = new(graph);
			annotations.Add(annotation);
		}
		public void AddControlFlowBlock(IControlFlowBlock block)
		{
			ControlFlowBlockAnnotation annotation = new(block);
			annotations.Add(annotation);
		}

		public IControlFlowGraph? TryGetControlFlowGraph() => annotations.TryGet<ControlFlowGraphAnnotation>()?.Graph;
		public IControlFlowGraph GetControlFlowGraph() => annotations.Get<ControlFlowGraphAnnotation>().Graph;
		public IControlFlowBlock GetControlFlowBlock() => annotations.Get<ControlFlowBlockAnnotation>().Block;
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
					IAnnotatedBlockFunctionBodySyntax block => block.Block.Annotations.GetControlFlowGraph().AlwaysReturnsValue,
					IAnnotatedShortFunctionBodySyntax => true,
					IAnnotatedOnlyTerminatedFunctionBodySyntax => false,

					_ => ThrowHelper.ThrowInvalidOperationException<bool>($"Unknown function body type '{function.Body.GetType().Name}'.")
				};
			}
		}
		#endregion
	}
}
