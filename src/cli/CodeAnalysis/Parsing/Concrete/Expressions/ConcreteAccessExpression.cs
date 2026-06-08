namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Concrete.Expressions;

public sealed class ConcreteAccessExpression : BaseConcreteSyntaxNode, IConcreteExpression
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.Access;
	public ITokenNode<string> Name { get; }
	#endregion

	#region Constructors
	public ConcreteAccessExpression(ITokenNode<string> name)
	{
		Name = name;
	}
	#endregion

	#region Methods
	public override IEnumerable<IConcreteSyntaxNode> GetChildren() => [Name];
	#endregion
}
