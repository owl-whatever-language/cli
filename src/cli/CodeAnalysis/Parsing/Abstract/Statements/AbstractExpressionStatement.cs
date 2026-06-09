namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Abstract.Statements;

public sealed class AbstractExpressionStatement : BaseAbstractStatement<ConcreteExpressionStatement>
{
	#region Properties
	public IAbstractExpression Expression { get; }
	#endregion

	#region Constructors
	public AbstractExpressionStatement(ConcreteExpressionStatement concrete, IAbstractExpression expression) : base(concrete)
	{
		Expression = expression;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISyntaxNode> GetChildren() => [Expression];
	#endregion
}
