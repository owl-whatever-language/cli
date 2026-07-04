namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Graphs;

public interface IDocumentControlFlowGraph : IControlFlowGraph
{
	#region Properties
	new IAnnotatedDocumentSyntax Node { get; }
	IAnnotatedSyntaxNode IControlFlowGraph.Node => Node;
	#endregion
}

public sealed class DocumentControlFlowGraph : ControlFlowGraph<IAnnotatedDocumentSyntax>, IDocumentControlFlowGraph
{
	#region Constructors
	public DocumentControlFlowGraph(IAnnotatedDocumentSyntax node) : base(node) { }
	#endregion
}
