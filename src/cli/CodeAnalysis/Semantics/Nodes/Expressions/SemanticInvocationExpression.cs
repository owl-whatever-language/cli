namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes.Expressions;

public sealed class SemanticInvocationExpression : BaseSemanticExpression<AbstractInvocationExpression>
{
	#region Properties
	public ISemanticExpression Expression { get; }
	public ISemanticExpression Value { get; }
	public IFunctionInfo? Function { get; }
	#endregion

	#region Constructors
	public SemanticInvocationExpression(
		AbstractInvocationExpression @abstract,
		ITypeInfo? type,
		ISemanticExpression expression,
		ISemanticExpression value,
		IFunctionInfo? function)
		: base(@abstract, type)
	{
		Expression = expression;
		Value = value;
		Function = function;
	}
	#endregion

	#region Methods
	public override IEnumerable<ISemanticSyntaxNode> GetChildren() => [Expression, Value];
	#endregion
}
