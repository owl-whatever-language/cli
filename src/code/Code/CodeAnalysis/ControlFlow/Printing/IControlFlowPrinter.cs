using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Graphs;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Printing;

public interface IControlFlowPrinter
{
	#region Methods
	object Print(IControlFlowGraph graph);
	#endregion
}

public interface IControlFlowPrinter<out TResult> : IControlFlowPrinter
	where TResult : notnull
{
	#region Methods
	new TResult Print(IControlFlowGraph graph);
	object IControlFlowPrinter.Print(IControlFlowGraph graph) => Print(graph);
	#endregion
}

public interface ICustomisableControlFlowPrinter<in TSettings>
	where TSettings : notnull, IControlFlowPrinterSettings
{
	#region Methods
	object Print(IControlFlowGraph graph, TSettings settings);
	#endregion
}


public interface ICustomisableControlFlowPrinter<in TSettings, out TResult> : ICustomisableControlFlowPrinter<TSettings>
	where TSettings : notnull, IControlFlowPrinterSettings
	where TResult : notnull
{
	#region Methods
	new TResult Print(IControlFlowGraph graph, TSettings settings);
	object ICustomisableControlFlowPrinter<TSettings>.Print(IControlFlowGraph graph, TSettings settings) => Print(graph, settings);
	#endregion
}
