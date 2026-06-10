namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes.Expressions;

public sealed class SemanticLiteralExpression : BaseSemanticExpression<AbstractLiteralExpression>
{
	#region Properties
	public object? Value { get; }
	#endregion

	#region Constructors
	public SemanticLiteralExpression(AbstractLiteralExpression @abstract, ITypeInfo? type, object? value) : base(@abstract, type)
	{
		Value = value;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISemanticSyntaxNode> GetChildren() => [];
	#endregion
}
