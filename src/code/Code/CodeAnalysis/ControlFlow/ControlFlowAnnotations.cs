using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;
using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Graphs;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow;

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

public static class ControlFlowAnnotationExtensions
{
	extension(IAnnotatedSyntaxNode node)
	{
		#region Methods
		public bool TryGetControlFlowBlock([NotNullWhen(true)] out IControlFlowBlock? block)
		{
			return node switch
			{
				IAnnotatedStatementSyntax statement => statement.TryGetControlFlowBlock(out block),
				IAnnotatedExpressionSyntax expression => expression.TryGetControlFlowBlock(out block),

				_ => TryGetGeneralControlFlowBlock(node, out block)
			};
		}
		public bool TryGetControlFlowGraph([NotNullWhen(true)] out IControlFlowGraph? graph)
		{
			return node switch
			{
				IAnnotatedDocumentSyntax document => document.TryGetControlFlowGraph(out graph),
				IAnnotatedFunctionDeclarationStatementSyntax function => function.TryGetControlFlowGraph(out graph),
				IAnnotatedLocalFunctionDeclarationStatementSyntax function => function.Declaration.TryGetControlFlowGraph(out graph),

				_ => TryGetGeneralControlFlowGraph(node, out graph)
			};
		}

		public void AttachGeneralControlFlowBlock(IControlFlowBlock block)
		{
			ControlFlowBlockAnnotation annotation = new(block);
			node.Annotations.Add(annotation);
		}
		#endregion
	}

	extension(IAnnotatedStatementSyntax statement)
	{
		#region Methods
		public void AttachControlFlowBlock(IControlFlowStatementBlock block) => AttachGeneralControlFlowBlock(statement, block);
		public bool TryGetControlFlowBlock([NotNullWhen(true)] out IControlFlowStatementBlock? block)
		{
			if (TryGetGeneralControlFlowBlock(statement, out IControlFlowBlock? general))
			{
				block = (IControlFlowStatementBlock)general;
				return true;
			}

			block = default;
			return false;
		}
		#endregion
	}

	extension(IAnnotatedSyntaxTree tree)
	{
		#region Methods
		public IReadOnlyList<IControlFlowGraph> CollectControlFlowGraphs()
		{
			return tree
				.CollectAnnotations<ControlFlowGraphAnnotation>()
				.Select(a => a.Graph)
				.ToArray();
		}
		#endregion
	}

	extension(IAnnotatedExpressionSyntax expression)
	{
		#region Methods
		public void AttachControlFlowBlock(IControlFlowExpressionBlock block) => AttachGeneralControlFlowBlock(expression, block);
		public bool TryGetControlFlowBlock([NotNullWhen(true)] out IControlFlowExpressionBlock? block)
		{
			if (TryGetGeneralControlFlowBlock(expression, out IControlFlowBlock? general))
			{
				block = (IControlFlowExpressionBlock)general;
				return true;
			}

			block = default;
			return false;
		}
		#endregion
	}

	extension(IAnnotatedDocumentSyntax document)
	{
		#region Methods
		public bool TryGetControlFlowGraph([NotNullWhen(true)] out IDocumentControlFlowGraph? graph)
		{
			if (document.Annotations.TryGet(out ControlFlowGraphAnnotation? annotation))
			{
				graph = (IDocumentControlFlowGraph)annotation.Graph;
				return true;
			}

			graph = default;
			return false;
		}
		#endregion
	}

	extension(IAnnotatedFunctionDeclarationStatementSyntax function)
	{
		#region Methods
		public bool TryGetControlFlowGraph([NotNullWhen(true)] out IFunctionControlFlowGraph? graph)
		{
			if (function.Annotations.TryGet(out ControlFlowGraphAnnotation? annotation))
			{
				graph = (IFunctionControlFlowGraph)annotation.Graph;
				return true;
			}

			graph = default;
			return false;
		}
		#endregion
	}

	extension(IAnnotatedLocalFunctionDeclarationStatementSyntax function)
	{
		#region Methods
		public bool TryGetControlFlowGraph([NotNullWhen(true)] out IFunctionControlFlowGraph? graph)
		{
			return function.Declaration.TryGetControlFlowGraph(out graph);
		}
		#endregion
	}

	#region Helpers
	private static bool TryGetGeneralControlFlowBlock(IAnnotatedSyntaxNode node, [NotNullWhen(true)] out IControlFlowBlock? block)
	{
		if (node.Annotations.TryGet(out ControlFlowBlockAnnotation? annotation))
		{
			block = annotation.Block;
			return true;
		}

		block = default;
		return false;
	}
	private static bool TryGetGeneralControlFlowGraph(IAnnotatedSyntaxNode node, [NotNullWhen(true)] out IControlFlowGraph? graph)
	{
		if (node.Annotations.TryGet(out ControlFlowGraphAnnotation? annotation))
		{
			graph = annotation.Graph;
			return true;
		}

		if (TryGetGeneralControlFlowBlock(node, out _))
		{
			ISyntaxNode? parent = node.Parent;
			while (parent is not null)
			{
				if (parent is IAnnotatedSyntaxNode annotated)
				{
					if (TryGetGeneralControlFlowGraph(annotated, out graph))
						return true;
				}

				parent = parent.Parent;
			}
		}

		graph = default;
		return false;
	}
	#endregion
}
