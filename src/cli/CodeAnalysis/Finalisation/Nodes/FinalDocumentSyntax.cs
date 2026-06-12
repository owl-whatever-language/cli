namespace OwlDomain.Owl.CLI.CodeAnalysis.Finalisation.Nodes;

public class FinalDocumentSyntax : BaseFinalSyntaxNode
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.Document;
	public IFinalSyntaxList<IFinalStatement> Statements { get; }
	public IFinalSyntaxToken EndOfInput { get; }
	#endregion

	#region Constructors
	public FinalDocumentSyntax(IFinalSyntaxList<IFinalStatement> statements, IFinalSyntaxToken endOfInput)
	{
		Statements = statements;
		EndOfInput = endOfInput;
	}
	#endregion

	#region Methods
	public override IEnumerable<IFinalSyntaxNode> GetChildren() => [Statements, EndOfInput];
	#endregion
}
