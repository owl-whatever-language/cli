namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Abstract;

public sealed class AbstractSyntaxTree : BaseAbstractSyntaxTree<ConcreteSyntaxTree, AbstractDocumentSyntax>
{
	#region Constructors
	public AbstractSyntaxTree(
		ISourceFile source,
		ConcreteSyntaxTree concrete,
		AbstractDocumentSyntax root)
		: base(source, concrete, root)
	{
	}
	#endregion
}
