namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Abstract.Expressions;

public sealed class AbstractAccessExpression : BaseAbstractExpression<ConcreteAccessExpression>
{
	#region Properties
	public IAbstractSyntaxToken Name { get; }
	#endregion

	#region Constructors
	public AbstractAccessExpression(ConcreteAccessExpression concrete, IAbstractSyntaxToken name) : base(concrete)
	{
		Name = name;
	}
	#endregion

	#region Methods
	public override IEnumerable<IAbstractSyntaxNode> GetChildren() => [Name];
	#endregion
}
