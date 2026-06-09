namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Abstract.Expressions;

public sealed class AbstractInvocationExpression : BaseAbstractExpression<ConcreteInvocationExpression>
{
	#region Properties
	public IAbstractExpression Expression { get; }
	public IAbstractExpression Value { get; }
	#endregion

	#region Constructors
	public AbstractInvocationExpression(
		ConcreteInvocationExpression concrete,
		IAbstractExpression expression,
		IAbstractExpression value)
		: base(concrete)
	{
		Expression = expression;
		Value = value;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISyntaxNode> GetChildren() => [Expression, Value];
	#endregion
}
