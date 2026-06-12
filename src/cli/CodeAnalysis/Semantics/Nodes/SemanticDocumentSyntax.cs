namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes;

public class SemanticDocumentSyntax : BaseSemanticSyntaxNode
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.Document;
	public ISemanticSyntaxList<ISemanticStatement> Statements { get; }
	public ISemanticSyntaxToken EndOfInput { get; }
	#endregion

	#region Constructors
	public SemanticDocumentSyntax(ISemanticSyntaxList<ISemanticStatement> statements, ISemanticSyntaxToken endOfInput)
	{
		Statements = statements;
		EndOfInput = endOfInput;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISemanticSyntaxNode> GetChildren() => [Statements, EndOfInput];
	#endregion
}
