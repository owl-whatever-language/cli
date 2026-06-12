namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes.Statements;

public sealed class SemanticExpressionStatement : BaseSemanticSyntaxNode, ISemanticTerminatedStatement
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
	public ISemanticExpression Expression { get; }
	public ISemanticSyntaxToken Terminator { get; }
	#endregion

	#region Constructors
	public SemanticExpressionStatement(ISemanticExpression expression, ISemanticSyntaxToken terminator)
	{
		Expression = expression;
		Terminator = terminator;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISemanticSyntaxNode> GetChildren() => [Expression, Terminator];
	#endregion
}
