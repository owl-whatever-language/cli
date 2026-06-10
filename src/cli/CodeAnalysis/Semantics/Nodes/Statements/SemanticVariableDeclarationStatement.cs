namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes.Statements;

public class SemanticVariableDeclarationStatement : BaseSemanticStatement<AbstractVariableDeclarationStatement>
{
	#region Properties
	public ITypeInfo? Type { get; }
	public LocalVariableSymbol Symbol { get; }
	public ISemanticExpression Value { get; }
	#endregion

	#region Constructors
	public SemanticVariableDeclarationStatement(
		AbstractVariableDeclarationStatement @abstract,
		ITypeInfo? type,
		LocalVariableSymbol symbol,
		ISemanticExpression value)
		: base(@abstract)
	{
		Type = type;
		Symbol = symbol;
		Value = value;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISemanticSyntaxNode> GetChildren() => [Value];
	#endregion
}
