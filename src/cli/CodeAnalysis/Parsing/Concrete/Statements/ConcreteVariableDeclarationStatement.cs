namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Concrete.Statements;

public sealed class ConcreteVariableDeclarationStatement : BaseConcreteSyntaxNode, ITerminatedConcreteStatement
{
	#region Properties
	public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;
	public ITokenNode TypeName { get; }
	public ITokenNode Name { get; }
	public ITokenNode Assignment { get; }
	public IConcreteExpression Value { get; }
	public ITokenNode Terminator { get; }
	#endregion

	#region Constructors
	public ConcreteVariableDeclarationStatement(
		ITokenNode typeName,
		ITokenNode name,
		ITokenNode assignment,
		IConcreteExpression value,
		ITokenNode terminator)
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
