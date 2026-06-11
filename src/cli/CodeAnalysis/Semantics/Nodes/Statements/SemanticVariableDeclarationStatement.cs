namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes.Statements;

public class SemanticVariableDeclarationStatement : BaseSemanticStatement<AbstractVariableDeclarationStatement>
{
	#region Properties
	public ILocalVariableTarget Target { get; }
	public ISemanticSyntaxToken TypeName { get; }
	public ISemanticSyntaxToken Name { get; }
	public ISemanticExpression Value { get; }
	#endregion

	#region Constructors
	public SemanticVariableDeclarationStatement(
		AbstractVariableDeclarationStatement @abstract,
		ISemanticSyntaxToken typeName,
		ISemanticSyntaxToken name,
		ISemanticExpression value,
		ILocalVariableTarget target)
		: base(@abstract)
	{
		TypeName = typeName;
		Name = name;
		Value = value;
		Target = target;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISemanticSyntaxNode> GetChildren() => [Value];
	#endregion
}
