namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes.Expressions;

public sealed class SemanticAccessExpression : BaseSemanticSyntaxNode, ISemanticExpression
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.Access;
	public ISemanticSyntaxToken Name { get; }
	public ITypeInfo? Type => Name.Type;
	#endregion

	#region Constructors
	public SemanticAccessExpression(ISemanticSyntaxToken name)
	{
		Name = name;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISemanticSyntaxNode> GetChildren() => [Name];
	#endregion
}
