namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Concrete.Expressions;

public sealed class ConcreteInvocationExpression : BaseConcreteSyntaxNode, IConcreteExpression
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.Invocation;
	public IConcreteExpression Expression { get; }
	public ITokenNode OpeningBracket { get; }
	public IConcreteExpression Value { get; }
	public ITokenNode ClosingBracket { get; }
	#endregion

	#region Constructors
	public ConcreteInvocationExpression(IConcreteExpression expression, ITokenNode openingBracket, IConcreteExpression value, ITokenNode closingBracket)
	{
		Expression = expression;
		OpeningBracket = openingBracket;
		Value = value;
		ClosingBracket = closingBracket;
	}
	#endregion

	#region Methods
	public override IEnumerable<IConcreteSyntaxNode> GetChildren() => [Expression, OpeningBracket, Value, ClosingBracket];
	#endregion
}
