namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Nodes;

public sealed class ConcreteDocumentSyntax : BaseConcreteSyntaxNode
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.Document;
	public IConcreteSyntaxList<IConcreteStatement> Statements { get; }
	public IConcreteSyntaxToken EndOfInput { get; }
	#endregion

	#region Constructors
	public ConcreteDocumentSyntax(IConcreteSyntaxList<IConcreteStatement> statements, IConcreteSyntaxToken endOfInput)
	{
		Statements = statements;
		EndOfInput = endOfInput;
	}
	#endregion

	#region Methods
	public override IEnumerable<IConcreteSyntaxNode> GetChildren() => [Statements, EndOfInput];
	#endregion
}
