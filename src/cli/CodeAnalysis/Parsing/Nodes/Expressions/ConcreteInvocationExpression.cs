namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Nodes.Expressions;

public sealed class ConcreteInvocationExpression : BaseConcreteSyntaxNode, IConcreteExpression
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.Invocation;
	public IConcreteExpression Expression { get; }
	public IConcreteSyntaxToken OpeningBracket { get; }
	public IConcreteSeparatedSyntaxList<IConcreteExpression, IConcreteSyntaxToken> Values { get; }
	public IConcreteSyntaxToken ClosingBracket { get; }
	#endregion

	#region Constructors
	public ConcreteInvocationExpression(
		IConcreteExpression expression,
		IConcreteSyntaxToken openingBracket,
		IConcreteSeparatedSyntaxList<IConcreteExpression, IConcreteSyntaxToken> values,
		IConcreteSyntaxToken closingBracket)
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
