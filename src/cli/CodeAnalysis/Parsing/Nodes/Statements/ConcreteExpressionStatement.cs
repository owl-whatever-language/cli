namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Nodes.Statements;

public sealed class ConcreteExpressionStatement : BaseConcreteSyntaxNode, IConcreteTerminatedStatement
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
	public IConcreteExpression Expression { get; }
	public IConcreteSyntaxToken Terminator { get; }
	#endregion

	#region Constructors
	public ConcreteExpressionStatement(IConcreteExpression expression, IConcreteSyntaxToken terminator)
	{
		Expression = expression;
		Terminator = terminator;
	}
	#endregion

	#region Methods
	public override IEnumerable<IConcreteSyntaxNode> GetChildren() => [Expression, Terminator];
	#endregion
}
