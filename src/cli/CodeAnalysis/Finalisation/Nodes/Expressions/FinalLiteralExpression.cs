namespace OwlDomain.Owl.CLI.CodeAnalysis.Finalisation.Nodes.Expressions;

public sealed class FinalLiteralExpression : BaseFinalSyntaxNode, IFinalExpression
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.Literal;
	public IFinalSyntaxToken Literal { get; }
	public ITypeInfo? Type => Literal.Type;
	#endregion

	#region Constructors
	public FinalLiteralExpression(IFinalSyntaxToken literal)
	{
		Literal = literal;
	}
	#endregion

	#region Methods
	public override IEnumerable<IFinalSyntaxNode> GetChildren() => [Literal];
	#endregion
}
