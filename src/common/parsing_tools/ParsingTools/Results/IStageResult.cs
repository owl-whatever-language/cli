namespace OwlDomain.ParsingTools.Results;

public enum ResultKind
{
	Regular = 0,
	Parallel,
}

public interface IStageResult
{
	#region Properties
	string Stage { get; }
	IDiagnosticBag Diagnostics { get; }
	IPerformanceResult Performance { get; }
	IReadOnlyList<IStageResult> SubResults { get; }
	ResultKind Kind { get; }
	#endregion
}

public abstract class StageResult : IStageResult
{
	#region Properties
	/// <inheritdoc/>
	public abstract string Stage { get; }

	/// <inheritdoc/>
	public IDiagnosticBag Diagnostics { get; }

	/// <inheritdoc/>
	public IPerformanceResult Performance { get; }

	/// <inheritdoc/>
	public IReadOnlyList<IStageResult> SubResults { get; }

	/// <inheritdoc/>
	public ResultKind Kind { get; }
	#endregion

	#region Constructors
	protected StageResult(
		IDiagnosticBag diagnostics,
		IPerformanceResult performance,
		IReadOnlyList<IStageResult> subResults,
		ResultKind kind)
	{
		Diagnostics = diagnostics;
		Performance = performance;
		SubResults = subResults;
		Kind = kind;
	}

	protected StageResult(
		IDiagnosticBag diagnostics,
		IPerformanceResult performance,
		IReadOnlyList<IStageResult> subResults)
		: this(diagnostics, performance, subResults, default) { }

	protected StageResult(
		IDiagnosticBag diagnostics,
		IPerformanceResult performance)
		: this(diagnostics, performance, [], default) { }
	#endregion
}
