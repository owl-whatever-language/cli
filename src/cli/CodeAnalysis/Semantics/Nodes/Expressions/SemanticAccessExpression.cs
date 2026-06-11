namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes.Expressions;

public sealed class SemanticAccessExpression : BaseSemanticExpression<AbstractAccessExpression>
{
	#region Properties
	public ISemanticSyntaxToken Name { get; }
	#endregion

	#region Constructors
	public SemanticAccessExpression(AbstractAccessExpression @abstract, ISemanticSyntaxToken name, ITypeInfo? type) : base(@abstract, type)
	{
		Name = name;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISemanticSyntaxNode> GetChildren() => [];
	#endregion
}
