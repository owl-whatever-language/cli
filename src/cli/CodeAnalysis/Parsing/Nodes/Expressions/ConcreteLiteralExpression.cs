namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Nodes.Expressions;

public sealed class ConcreteLiteralExpression : BaseConcreteSyntaxNode, IConcreteExpression
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.Literal;
	public IConcreteSyntaxToken Literal { get; }
	#endregion

	#region Constructors
	public ConcreteLiteralExpression(IConcreteSyntaxToken literal)
	{
		Literal = literal;
	}
	#endregion

	#region Methods
	public override IEnumerable<IConcreteSyntaxNode> GetChildren() => [Literal];
	#endregion
}
