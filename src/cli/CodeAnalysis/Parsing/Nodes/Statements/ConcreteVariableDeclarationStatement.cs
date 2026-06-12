namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Nodes.Statements;

public sealed class ConcreteVariableDeclarationStatement : BaseConcreteSyntaxNode, IConcreteTerminatedStatement
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;
	public IConcreteSyntaxToken TypeName { get; }
	public IConcreteSyntaxToken Name { get; }
	public IConcreteSyntaxToken Assignment { get; }
	public IConcreteExpression Value { get; }
	public IConcreteSyntaxToken Terminator { get; }
	#endregion

	#region Constructors
	public ConcreteVariableDeclarationStatement(
		IConcreteSyntaxToken typeName,
		IConcreteSyntaxToken name,
		IConcreteSyntaxToken assignment,
		IConcreteExpression value,
		IConcreteSyntaxToken terminator)
	{
		TypeName = typeName;
		Name = name;
		Assignment = assignment;
		Value = value;
		Terminator = terminator;
	}
	#endregion

	#region Methods
	public override IEnumerable<IConcreteSyntaxNode> GetChildren() => [TypeName, Name, Assignment, Value, Terminator];
	#endregion
}
