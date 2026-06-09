namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Abstract;

public sealed class AbstractDocumentSyntax : BaseAbstractSyntaxNode<ConcreteDocumentSyntax>
{
	#region Properties
	public IReadOnlyList<IAbstractStatement> Statements { get; }
	#endregion

	#region Constructors
	public AbstractDocumentSyntax(ConcreteDocumentSyntax concrete, IReadOnlyList<IAbstractStatement> statements) : base(concrete)
	{
		Statements = statements;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISyntaxNode> GetChildren() => Statements;
	#endregion
}
