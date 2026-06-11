namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Abstract.Expressions;

public sealed class AbstractInvocationExpression : BaseAbstractExpression<ConcreteInvocationExpression>
{
	#region Properties
	public IAbstractExpression Expression { get; }
	public IAbstractSyntaxList<IAbstractExpression> Values { get; }
	#endregion

	#region Constructors
	public AbstractInvocationExpression(
		ConcreteInvocationExpression concrete,
		IAbstractExpression expression,
		IAbstractSyntaxList<IAbstractExpression> values)
		: base(concrete)
	{
		Expression = expression;
		Values = values;
	}
	#endregion

	#region Methods
	public override IEnumerable<IAbstractSyntaxNode> GetChildren() => [Expression, Values];
	#endregion
}
