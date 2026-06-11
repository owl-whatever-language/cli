namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Abstract;

public sealed class AbstractDocumentSyntax : BaseAbstractSyntaxNode<ConcreteDocumentSyntax>
{
	#region Properties
	public IAbstractSyntaxList<IAbstractStatement> Statements { get; }
	#endregion

	#region Constructors
	public AbstractDocumentSyntax(ConcreteDocumentSyntax concrete, IAbstractSyntaxList<IAbstractStatement> statements) : base(concrete)
	{
		Statements = statements;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISyntaxNode> GetChildren() => [Statements];
	#endregion
}
