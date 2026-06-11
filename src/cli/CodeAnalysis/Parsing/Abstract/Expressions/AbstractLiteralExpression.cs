namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Abstract.Expressions;

public sealed class AbstractLiteralExpression : BaseAbstractExpression<ConcreteLiteralExpression>
{
	#region Properties
	public IAbstractSyntaxToken Literal { get; }
	#endregion

	#region Constructors
	public AbstractLiteralExpression(ConcreteLiteralExpression concrete, IAbstractSyntaxToken literal) : base(concrete)
	{
		Literal = literal;
	}
	#endregion

	#region Methods
	public override IEnumerable<IAbstractSyntaxNode> GetChildren() => [Literal];
	#endregion
}
