namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Concrete.Expressions;

public sealed class ConcreteInvocationExpression : BaseConcreteSyntaxNode, IConcreteExpression
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.Invocation;
	public IConcreteExpression Expression { get; }
	public ITokenNode OpeningBracket { get; }
	public IConcreteSeparatedSyntaxList<IConcreteExpression, ITokenNode> Values { get; }
	public ITokenNode ClosingBracket { get; }
	#endregion

	#region Constructors
	public ConcreteInvocationExpression(
		IConcreteExpression expression,
		ITokenNode openingBracket,
		IConcreteSeparatedSyntaxList<IConcreteExpression, ITokenNode> values,
		ITokenNode closingBracket)
	{
		Expression = expression;
		OpeningBracket = openingBracket;
		Values = values;
		ClosingBracket = closingBracket;
	}
	#endregion

	#region Methods
	public override IEnumerable<IConcreteSyntaxNode> GetChildren() => [Expression, OpeningBracket, Values, ClosingBracket];
	#endregion
}
