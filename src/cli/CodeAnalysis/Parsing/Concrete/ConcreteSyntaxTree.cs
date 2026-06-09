namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Concrete;

public sealed class ConcreteSyntaxTree : BaseConcreteSyntaxTree<ConcreteDocumentSyntax>
{
	#region Properties
	public ConcreteSyntaxTree(ISourceFile source, ConcreteDocumentSyntax root) : base(source, root) { }
	#endregion
}
