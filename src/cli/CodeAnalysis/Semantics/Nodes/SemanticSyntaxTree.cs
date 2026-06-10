namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes;

public sealed class SemanticSyntaxTree : BaseSemanticSyntaxTree<AbstractSyntaxTree, SemanticDocumentSyntax>
{
	#region Constructors
	public SemanticSyntaxTree(
		ISourceFile source,
		AbstractSyntaxTree @abstract,
		SemanticDocumentSyntax document)
		: base(source, @abstract, document)
	{
	}
	#endregion
}
