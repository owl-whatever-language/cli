namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes;

public class SemanticDocumentSyntax : BaseSemanticSyntaxNode<AbstractDocumentSyntax>
{
	#region Properties
	public IReadOnlyList<ISemanticStatement> Statements { get; }
	#endregion

	#region Constructors
	public SemanticDocumentSyntax(AbstractDocumentSyntax @abstract, IReadOnlyList<ISemanticStatement> statements) : base(@abstract)
	{
		Statements = statements;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISemanticSyntaxNode> GetChildren() => Statements;
	#endregion
}
