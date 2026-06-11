namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Abstract.Statements;

public sealed class AbstractVariableDeclarationStatement : BaseAbstractStatement<ConcreteVariableDeclarationStatement>
{
	#region Properties
	public IAbstractSyntaxToken TypeName { get; }
	public IAbstractSyntaxToken Name { get; }
	public IAbstractExpression Value { get; }
	#endregion

	#region Constructors
	public AbstractVariableDeclarationStatement(
		ConcreteVariableDeclarationStatement concrete,
		IAbstractSyntaxToken typeName,
		IAbstractSyntaxToken name,
		IAbstractExpression value)
		: base(concrete)
	{
		TypeName = typeName;
		Name = name;
		Value = value;
	}
	#endregion

	#region Methods
	public override IEnumerable<IAbstractSyntaxNode> GetChildren() => [TypeName, Name, Value];
	#endregion
}
