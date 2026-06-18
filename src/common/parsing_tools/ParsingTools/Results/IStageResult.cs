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

public static class IStageResultExtensions
{
	#region Functions
	private static void AppendDiagnostics(List<IDiagnostic> target, IStageResult current)
	{
		target.AddRange(current.Diagnostics);

		foreach (IStageResult child in current.SubResults)
			AppendDiagnostics(target, child);
	}
	#endregion

	extension(IStageResult result)
	{
		#region Methods
		public IReadOnlyCollection<IDiagnostic> GetAllDiagnostics()
		{
			List<IDiagnostic> diagnostics = [];
			AppendDiagnostics(diagnostics, result);

			return diagnostics;
		}
		#endregion
	}

	extension(IEnumerable<IStageResult> results)
	{
		#region Methods
		public IReadOnlyCollection<IDiagnostic> GetAllDiagnostics()
		{
			List<IDiagnostic> diagnostics = [];

			foreach (IStageResult result in results)
				AppendDiagnostics(diagnostics, result);

			return diagnostics;
		}
		#endregion
	}
}
