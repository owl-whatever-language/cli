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
