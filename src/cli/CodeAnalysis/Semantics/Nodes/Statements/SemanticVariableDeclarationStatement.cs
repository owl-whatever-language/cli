namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes.Statements;

public sealed class SemanticVariableDeclarationStatement : BaseSemanticSyntaxNode, ISemanticTerminatedStatement
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;
	public ISemanticSyntaxToken TypeName { get; }
	public ISemanticSyntaxToken Name { get; }
	public ISemanticSyntaxToken Assignment { get; }
	public ISemanticExpression Value { get; }
	public ISemanticSyntaxToken Terminator { get; }
	#endregion

	#region Constructors
	public SemanticVariableDeclarationStatement(
		ISemanticSyntaxToken typeName,
		ISemanticSyntaxToken name,
		ISemanticSyntaxToken assignment,
		ISemanticExpression value,
		ISemanticSyntaxToken terminator)
	{
		TypeName = typeName;
		Name = name;
		Assignment = assignment;
		Value = value;
		Terminator = terminator;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISemanticSyntaxNode> GetChildren() => [TypeName, Name, Assignment, Value, Terminator];
	#endregion
}
