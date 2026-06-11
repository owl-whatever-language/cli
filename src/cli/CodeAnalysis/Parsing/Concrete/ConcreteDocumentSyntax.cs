namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Concrete;

public sealed class ConcreteDocumentSyntax : BaseConcreteSyntaxNode
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.Document;
	public IConcreteSyntaxList<IConcreteStatement> Statements { get; }
	public ITokenNode EndOfInput { get; }
	#endregion

	#region Constructors
	public ConcreteDocumentSyntax(IConcreteSyntaxList<IConcreteStatement> statements, ITokenNode endOfInput)
	{
		Statements = statements;
		EndOfInput = endOfInput;
	}
	#endregion

	#region Methods
	public override IEnumerable<IConcreteSyntaxNode> GetChildren() => [Statements, EndOfInput];
	#endregion
}
