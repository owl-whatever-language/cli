namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Concrete.Expressions;

public sealed class ConcreteLiteralExpression : BaseConcreteSyntaxNode, IConcreteExpression
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.Literal;
	public ITokenNode Literal { get; }
	#endregion

	#region Constructors
	public ConcreteLiteralExpression(ITokenNode literal)
	{
		Literal = literal;
	}
	#endregion

	#region Methods
	public override IEnumerable<IConcreteSyntaxNode> GetChildren() => [Literal];
	#endregion
}
