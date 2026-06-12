namespace OwlDomain.Owl.CLI.CodeAnalysis.Finalisation.Nodes.Expressions;

public sealed class FinalAccessExpression : BaseFinalSyntaxNode, IFinalExpression
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.Access;
	public IFinalSyntaxToken Name { get; }
	public ITypeInfo? Type => Name.Type;
	#endregion

	#region Constructors
	public FinalAccessExpression(IFinalSyntaxToken name)
	{
		Name = name;
	}
	#endregion

	#region Methods
	public override IEnumerable<IFinalSyntaxNode> GetChildren() => [Name];
	#endregion
}
