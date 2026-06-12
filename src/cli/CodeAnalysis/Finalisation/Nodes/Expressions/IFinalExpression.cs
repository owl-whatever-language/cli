namespace OwlDomain.Owl.CLI.CodeAnalysis.Finalisation.Nodes.Expressions;

public interface IFinalExpression : IFinalSyntaxNode
{
	#region Properties
	ITypeInfo? Type { get; }
	#endregion
}
