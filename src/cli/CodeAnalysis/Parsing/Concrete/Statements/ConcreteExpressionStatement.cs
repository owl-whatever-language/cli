namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Concrete.Statements;

public sealed class ConcreteExpressionStatement : BaseConcreteSyntaxNode, ITerminatedConcreteStatement
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
	public IConcreteExpression Expression { get; }
	public ITokenNode Terminator { get; }
	#endregion

	#region Constructors
	public ConcreteExpressionStatement(IConcreteExpression expression, ITokenNode terminator)
	{
		Expression = expression;
		Terminator = terminator;
	}
	#endregion

	#region Methods
	public override IEnumerable<IConcreteSyntaxNode> GetChildren() => [Expression, Terminator];
	#endregion
}
