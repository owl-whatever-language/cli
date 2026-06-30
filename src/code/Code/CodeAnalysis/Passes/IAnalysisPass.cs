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

public static class AnalysisPass
{
	#region Result types
	public sealed class TreeResult : AnalysisPassResult, ISourceStageResult
	{
		#region Properties
		public IAnnotatedSyntaxTree Tree { get; }
		public ISourceFile Source => Tree.Source;
		#endregion

		#region Constructors
		public TreeResult(
			IAnalysisPass pass,
			IPerformanceResult performance,
			IDiagnosticBag diagnostics,
			IAnnotatedSyntaxTree tree)
			: base(pass, performance, diagnostics)
		{
			Tree = tree;
		}
		#endregion
	}
	public sealed class ParallelTreeResult : AnalysisPassResult, IParallelStageResult<TreeResult>
	{
		#region Properties
		public IReadOnlyCollection<TreeResult> Children { get; }
		#endregion

		#region Constructors
		public ParallelTreeResult(
			IAnalysisPass pass,
			IPerformanceResult performance,
			IDiagnosticBag diagnostics,
			IReadOnlyCollection<TreeResult> results)
			: base(pass, performance, diagnostics)
		{
			Children = results;
		}
		#endregion
	}
	#endregion
}
