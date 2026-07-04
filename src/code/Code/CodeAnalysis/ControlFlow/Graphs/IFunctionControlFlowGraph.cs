namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Graphs;

public interface IFunctionControlFlowGraph : IControlFlowGraph
{
	#region Properties
	new IAnnotatedFunctionDeclarationStatementSyntax Node { get; }
	IAnnotatedSyntaxNode IControlFlowGraph.Node => Node;
	#endregion
}

public sealed class FunctionControlFlowGraph : ControlFlowGraph<IAnnotatedFunctionDeclarationStatementSyntax>, IFunctionControlFlowGraph
{
	#region Constructors
	public FunctionControlFlowGraph(IAnnotatedFunctionDeclarationStatementSyntax node) : base(node) { }
	#endregion
}
