namespace OwlDomain.ParsingTools.Results;

public interface ISourceStageResult : IStageResult
{
	#region Properties
	ISourceFile Source { get; }
	#endregion
}

public abstract class SourceStageResult : StageResult, ISourceStageResult
{
	#region Properties
	public ISourceFile Source { get; }
	#endregion

	#region Constructors
	protected SourceStageResult(
		IDiagnosticBag diagnostics,
		IPerformanceResult performance,
		ISourceFile source)
		: base(diagnostics, performance)
	{
		Source = source;
	}

	protected SourceStageResult(
		IDiagnosticBag diagnostics,
		IPerformanceResult performance,
		ISourceFile source,
		IReadOnlyList<IStageResult> subResults)
		: base(diagnostics, performance, subResults)
	{
		Source = source;
	}

	protected SourceStageResult(
		IDiagnosticBag diagnostics,
		IPerformanceResult performance,
		ISourceFile source,
		IReadOnlyList<IStageResult> subResults,
		ResultKind kind)
		: base(diagnostics, performance, subResults, kind)
	{
		Source = source;
	}
	#endregion
}