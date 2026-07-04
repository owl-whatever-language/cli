using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Graphs;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Printing;

public interface IControlFlowPrinter
{
	#region Methods
	object Print(IControlFlowGraph graph);
	#endregion
}

public interface IControlFlowPrinter<out TOutput> : IControlFlowPrinter
	where TOutput : notnull
{
	#region Methods
	new TOutput Print(IControlFlowGraph graph);
	object IControlFlowPrinter.Print(IControlFlowGraph graph) => Print(graph);
	#endregion
}
