namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes.Expressions;

public sealed class SemanticInvocationExpression : BaseSemanticSyntaxNode, ISemanticExpression
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.Invocation;
	public ISemanticExpression Expression { get; }
	public ISemanticSyntaxToken OpeningBracket { get; }
	public ISemanticSeparatedSyntaxList<ISemanticExpression, ISemanticSyntaxToken> Values { get; }
	public ISemanticSyntaxToken ClosingBracket { get; }
	public IFunctionInfo? Function => (Expression.Type as FunctionType)?.Function;
	public ITypeInfo? Type => Expression.Type.ReturnType;
	#endregion

	#region Constructors
	public SemanticInvocationExpression(
		ISemanticExpression expression,
		ISemanticSyntaxToken openingBracket,
		ISemanticSeparatedSyntaxList<ISemanticExpression, ISemanticSyntaxToken> values,
		ISemanticSyntaxToken closingBracket)
	{
		Expression = expression;
		OpeningBracket = openingBracket;
		Values = values;
		ClosingBracket = closingBracket;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISemanticSyntaxNode> GetChildren() => [Expression, OpeningBracket, Values, ClosingBracket];
	#endregion
}
