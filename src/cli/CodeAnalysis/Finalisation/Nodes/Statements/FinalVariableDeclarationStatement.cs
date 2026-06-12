namespace OwlDomain.Owl.CLI.CodeAnalysis.Finalisation.Nodes.Statements;

public sealed class FinalVariableDeclarationStatement : BaseFinalSyntaxNode, IFinalTerminatedStatement
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;
	public IFinalSyntaxToken TypeName { get; }
	public IFinalSyntaxToken Name { get; }
	public IFinalSyntaxToken Assignment { get; }
	public IFinalExpression Value { get; }
	public IFinalSyntaxToken Terminator { get; }
	#endregion

	#region Constructors
	public FinalVariableDeclarationStatement(
		IFinalSyntaxToken typeName,
		IFinalSyntaxToken name,
		IFinalSyntaxToken assignment,
		IFinalExpression value,
		IFinalSyntaxToken terminator)
	{
		TypeName = typeName;
		Name = name;
		Assignment = assignment;
		Value = value;
		Terminator = terminator;
	}
	#endregion

	#region Methods
	public override IEnumerable<IFinalSyntaxNode> GetChildren() => [TypeName, Name, Assignment, Value, Terminator];
	#endregion
}
