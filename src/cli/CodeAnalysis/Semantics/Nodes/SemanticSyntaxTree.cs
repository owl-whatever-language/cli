namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes;

public sealed class SemanticSyntaxTree : BaseSemanticSyntaxTree<SemanticDocumentSyntax>
{
	#region Constructors
	public SemanticSyntaxTree(
		ISourceFile source,
		SemanticDocumentSyntax document)
		: base(source, document)
	{
	}
	#endregion
}
