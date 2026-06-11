namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes;

public class SemanticDocumentSyntax : BaseSemanticSyntaxNode<AbstractDocumentSyntax>
{
	#region Properties
	public ISemanticSyntaxList<ISemanticStatement> Statements { get; }
	#endregion

	#region Constructors
	public SemanticDocumentSyntax(AbstractDocumentSyntax @abstract, ISemanticSyntaxList<ISemanticStatement> statements) : base(@abstract)
	{
		Statements = statements;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISemanticSyntaxNode> GetChildren() => [Statements];
	#endregion
}
