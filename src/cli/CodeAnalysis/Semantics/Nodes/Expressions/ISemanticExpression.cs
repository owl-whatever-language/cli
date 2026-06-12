namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Nodes.Expressions;

public interface ISemanticExpression : ISemanticSyntaxNode
{
	#region Properties
	ITypeInfo? Type { get; }
	#endregion
}
