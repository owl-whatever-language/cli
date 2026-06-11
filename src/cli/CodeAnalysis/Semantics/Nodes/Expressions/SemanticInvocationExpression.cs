namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes.Expressions;

public sealed class SemanticInvocationExpression : BaseSemanticExpression<AbstractInvocationExpression>
{
	#region Properties
	public ISemanticExpression Expression { get; }
	public ISemanticSyntaxList<ISemanticExpression> Values { get; }
	public IFunctionInfo? Function { get; }
	#endregion

	#region Constructors
	public SemanticInvocationExpression(
		AbstractInvocationExpression @abstract,
		ITypeInfo? type,
		ISemanticExpression expression,
		ISemanticSyntaxList<ISemanticExpression> values,
		IFunctionInfo? function)
		: base(@abstract, type)
	{
		Expression = expression;
		Values = values;
		Function = function;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISemanticSyntaxNode> GetChildren() => [Expression, Values];
	#endregion
}
