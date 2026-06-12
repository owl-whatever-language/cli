namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Nodes.Expressions;

public sealed class ConcreteAccessExpression : BaseConcreteSyntaxNode, IConcreteExpression
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.Access;
	public IConcreteSyntaxToken Name { get; }
	#endregion

	#region Constructors
	public ConcreteAccessExpression(IConcreteSyntaxToken name)
	{
		Name = name;
	}
	#endregion

	#region Methods
	public override IEnumerable<IConcreteSyntaxNode> GetChildren() => [Name];
	#endregion
}
