namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes.Expressions;

public sealed class SemanticLiteralExpression : BaseSemanticSyntaxNode, ISemanticExpression
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.Literal;
	public ISemanticSyntaxToken Literal { get; }
	public ITypeInfo? Type => Literal.Type;
	#endregion

	#region Constructors
	public SemanticLiteralExpression(ISemanticSyntaxToken literal)
	{
		Literal = literal;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISemanticSyntaxNode> GetChildren() => [Literal];
	#endregion
}
