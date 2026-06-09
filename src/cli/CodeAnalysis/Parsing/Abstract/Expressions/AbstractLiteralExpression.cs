namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Abstract.Expressions;

public sealed class AbstractLiteralExpression : BaseAbstractExpression<ConcreteLiteralExpression>
{
	#region Properties
	public ITokenNode Literal => Concrete.Literal;
	public object? Value => Literal.Value;
	#endregion

	#region Constructors
	public AbstractLiteralExpression(ConcreteLiteralExpression concrete) : base(concrete) { }
	#endregion

	#region Methods
	public override IEnumerable<ISyntaxNode> GetChildren() => [Literal];
	#endregion
}
