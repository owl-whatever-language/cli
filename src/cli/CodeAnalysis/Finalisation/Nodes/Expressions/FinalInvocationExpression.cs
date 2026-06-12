namespace OwlDomain.Owl.CLI.CodeAnalysis.Finalisation.Nodes.Expressions;

public sealed class FinalInvocationExpression : BaseFinalSyntaxNode, IFinalExpression
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.Invocation;
	public IFinalExpression Expression { get; }
	public IFinalSyntaxToken OpeningBracket { get; }
	public IFinalSeparatedSyntaxList<IFinalExpression, IFinalSyntaxToken> Values { get; }
	public IFinalSyntaxToken ClosingBracket { get; }
	public IFunctionInfo? Function => (Expression.Type as FunctionType)?.Function;
	public ITypeInfo? Type => Expression.Type.ReturnType;
	#endregion

	#region Constructors
	public FinalInvocationExpression(
		IFinalExpression expression,
		IFinalSyntaxToken openingBracket,
		IFinalSeparatedSyntaxList<IFinalExpression, IFinalSyntaxToken> values,
		IFinalSyntaxToken closingBracket)
	{
		Expression = expression;
		OpeningBracket = openingBracket;
		Values = values;
		ClosingBracket = closingBracket;
	}
	#endregion

	#region Methods
	public override IEnumerable<IFinalSyntaxNode> GetChildren() => [Expression, OpeningBracket, Values, ClosingBracket];
	#endregion
}
