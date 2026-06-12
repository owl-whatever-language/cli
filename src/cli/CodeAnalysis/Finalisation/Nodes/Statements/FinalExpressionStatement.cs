namespace OwlDomain.Owl.CLI.CodeAnalysis.Finalisation.Nodes.Statements;

public sealed class FinalExpressionStatement : BaseFinalSyntaxNode, IFinalTerminatedStatement
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
	public IFinalExpression Expression { get; }
	public IFinalSyntaxToken Terminator { get; }
	#endregion

	#region Constructors
	public FinalExpressionStatement(IFinalExpression expression, IFinalSyntaxToken terminator)
	{
		Expression = expression;
		Terminator = terminator;
	}
	#endregion

	#region Methods
	public override IEnumerable<IFinalSyntaxNode> GetChildren() => [Expression, Terminator];
	#endregion
}
