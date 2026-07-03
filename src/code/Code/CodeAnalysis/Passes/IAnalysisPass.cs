namespace OwlDomain.Owl.Code.CodeAnalysis.Passes;

public interface IAnalysisPass
{
	#region Properties
	string Kind { get; }
	#endregion

	#region Methods
	IAnalysisPassResult Run(IAnalysisContext context);
	#endregion
}

public interface IAnalysisPass<out TResult> : IAnalysisPass
	where TResult : notnull, IAnalysisPassResult
{
	#region Methods
	new TResult Run(IAnalysisContext context);
	IAnalysisPassResult IAnalysisPass.Run(IAnalysisContext context) => Run(context);
	#endregion
}
