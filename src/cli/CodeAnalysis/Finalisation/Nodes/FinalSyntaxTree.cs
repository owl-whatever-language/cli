namespace OwlDomain.Owl.CLI.CodeAnalysis.Finalisation.Nodes;

public sealed class FinalSyntaxTree : BaseFinalSyntaxTree<FinalDocumentSyntax>
{
	#region Constructors
	public FinalSyntaxTree(
		ISourceFile source,
		FinalDocumentSyntax document)
		: base(source, document)
	{
	}
	#endregion
}
