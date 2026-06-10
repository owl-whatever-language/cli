namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes.Statements;

public class SemanticExpressionStatement : BaseSemanticStatement<AbstractExpressionStatement>
{
	#region Properties
	public ISemanticExpression Expression { get; }
	#endregion

	#region Constructors
	public SemanticExpressionStatement(AbstractExpressionStatement @abstract, ISemanticExpression expression) : base(@abstract)
	{
		Expression = expression;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISemanticSyntaxNode> GetChildren() => [Expression];
	#endregion
}
