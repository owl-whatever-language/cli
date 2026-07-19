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
	public abstract class PerTree : IAnalysisPass<ParallelAnalysisPassTreeResult>
	{
		#region Properties
		public abstract string Kind { get; }
		#endregion

		#region Methods
		public ParallelAnalysisPassTreeResult Run(IAnalysisContext context)
		{
			using (PerformanceResult.Scope(out IPerformanceResult performance))
			{
				IReadOnlyCollection<IAnnotatedSyntaxTree> trees = context.Annotated;

				if (trees.Count is 0)
					return new(this, performance, []);

				if (trees.Count is 1)
				{
					AnalysisPassTreeResult result = RunForTree(context, trees.Single());
					return new(this, performance, [result]);
				}

				AnalysisPassTreeResult[] results = new AnalysisPassTreeResult[trees.Count];

				ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };
				Parallel.ForEach(trees, options, (tree, _, index) => results[index] = RunForTree(context, tree));

				return new(this, performance, results);
			}
		}

		private AnalysisPassTreeResult RunForTree(IAnalysisContext context, IAnnotatedSyntaxTree tree)
		{
			using (PerformanceResult.Scope(out IPerformanceResult performance))
			{
				IDiagnosticBag diagnostics = Run(context, tree);

				return new(this, performance, diagnostics, tree);
			}
		}

		protected abstract IDiagnosticBag Run(IAnalysisContext context, IAnnotatedSyntaxTree tree);
		#endregion
	}

	public abstract class PerCompilation : IAnalysisPass<AnalysisPassResult>
	{
		#region Properties
		public abstract string Kind { get; }
		#endregion

		#region Methods
		public AnalysisPassResult Run(IAnalysisContext context)
		{
			using (PerformanceResult.Scope(out IPerformanceResult performance))
			{
				IDiagnosticBag diagnostics = RunCore(context);
				return new(this, performance, diagnostics);
			}
		}

		protected abstract IDiagnosticBag RunCore(IAnalysisContext context);
		#endregion
	}
}
