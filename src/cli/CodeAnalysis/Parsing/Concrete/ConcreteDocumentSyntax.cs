namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Concrete;

public sealed class ConcreteDocumentSyntax : BaseConcreteSyntaxNode
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.Document;
	public IReadOnlyList<IConcreteStatement> Statements { get; }
	public ITokenNode EndOfInput { get; }
	#endregion

	#region Constructors
	public ConcreteDocumentSyntax(IReadOnlyList<IConcreteStatement> statements, ITokenNode endOfInput)
	{
		Statements = statements;
		EndOfInput = endOfInput;
	}
	#endregion

	#region Methods
	public override IEnumerable<IConcreteSyntaxNode> GetChildren() => [.. Statements, EndOfInput];
	#endregion
}
