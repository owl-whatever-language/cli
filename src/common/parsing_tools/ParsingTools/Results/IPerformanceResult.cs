namespace OwlDomain.ParsingTools.Results;

public interface IPerformanceResult
{
	#region Properties
	TimeSpan SystemTime { get; }
	TimeSpan UserTime { get; }
	TimeSpan CpuTime { get; }
	TimeSpan Duration { get; }
	long MemoryUsed { get; }
	#endregion
}

internal sealed class PerformanceResultScope : IPerformanceResult
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly TimeSpan _systemTimeAtStart, _userTimeAtStart;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly long _memoryAtStart, _timestampAtStart;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private TimeSpan? _systemTimeAtEnd, _userTimeAtEnd;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private long? _memoryAtEnd, _timestampAtEnd;
	#endregion

	#region Properties
	/// <inheritdoc/>
	public TimeSpan SystemTime => _systemTimeAtEnd is null ? default : _systemTimeAtEnd.Value - _systemTimeAtStart;

	/// <inheritdoc/>
	public TimeSpan UserTime => _userTimeAtEnd is null ? default : _userTimeAtEnd.Value - _userTimeAtStart;

	/// <inheritdoc/>
	public TimeSpan CpuTime => SystemTime + UserTime;

	/// <inheritdoc/>
	public TimeSpan Duration => _timestampAtEnd is null ? default : Stopwatch.GetElapsedTime(_timestampAtStart, _timestampAtEnd.Value);

	/// <inheritdoc/>
	public long MemoryUsed => _memoryAtEnd is null ? 0 : Math.Max(0, _memoryAtEnd.Value - _memoryAtStart);
	#endregion

	#region Constructors
	internal PerformanceResultScope()
	{
		Environment.ProcessCpuUsage cpu = Environment.CpuUsage;
		_systemTimeAtStart = cpu.PrivilegedTime;
		_userTimeAtStart = cpu.UserTime;
		_memoryAtStart = GC.GetTotalMemory(true);
		_timestampAtStart = Stopwatch.GetTimestamp();
	}
	#endregion

	#region Methods
	internal void Stop()
	{
		Environment.ProcessCpuUsage cpu = Environment.CpuUsage;
		_systemTimeAtEnd = cpu.PrivilegedTime;
		_userTimeAtEnd = cpu.UserTime;
		_memoryAtEnd = GC.GetTotalMemory(true);
		_timestampAtEnd = Stopwatch.GetTimestamp();
	}
	#endregion
}

public readonly struct PerformanceScope : IDisposable
{
	#region Fields
	private readonly PerformanceResultScope _scope;
	#endregion

	#region Constructors
	internal PerformanceScope(PerformanceResultScope scope) => _scope = scope;
	#endregion

	#region Methods
	public void Dispose() => _scope.Stop();
	#endregion
}


public sealed class PerformanceResult : IPerformanceResult
{
	#region Properties
	public TimeSpan SystemTime { get; }
	public TimeSpan UserTime { get; }
	public TimeSpan CpuTime => SystemTime + UserTime;
	public TimeSpan Duration { get; }
	public long MemoryUsed { get; }
	#endregion

	#region Constructors
	public PerformanceResult(TimeSpan systemTime, TimeSpan userTime, TimeSpan duration, long memoryUsed)
	{
		SystemTime = systemTime;
		UserTime = userTime;
		Duration = duration;
		MemoryUsed = memoryUsed;
	}
	#endregion

	#region Functions
	public static PerformanceScope Scope(out IPerformanceResult result)
	{
		PerformanceResultScope scope = new();
		result = scope;

		return new(scope);
	}
	public static IReadOnlyDictionary<string, IPerformanceResult> CalculateStageBreakdown(
		IPerformanceResult parent,
		IEnumerable<IStageResultPerformance> results)
	{
		// Note(Nightowl):
		// I have absolutely no idea how mathematically sound this approach is for getting an estimate of parallelised results.
		// The approach that I'm taking here is to calculate the total values as if the result wasn't parallelised
		// in order to get a % share of the performance for a particular stage, and then I use that % on the true
		// parallelised performance result.

		Dictionary<string, IPerformanceResult> totals = [];

		foreach (IGrouping<string, IStageResultPerformance> group in results.GroupBy(s => s.Stage))
		{
			IPerformanceResult stageTotal = group.Select(g => g.Performance).Sum();
			totals.Add(group.Key, stageTotal);
		}

		IPerformanceResult total = totals.Values.Sum();

		Dictionary<string, IPerformanceResult> breakdowns = [];
		foreach (KeyValuePair<string, IPerformanceResult> pair in totals)
		{
			double systemShare = Correct(pair.Value.SystemTime / total.SystemTime);
			double userShare = Correct(pair.Value.UserTime / total.UserTime);
			double durationShare = Correct(pair.Value.Duration / total.Duration);
			double memoryShare = Correct(pair.Value.MemoryUsed / (double)total.MemoryUsed);

			TimeSpan system = systemShare * parent.SystemTime;
			TimeSpan user = userShare * parent.UserTime;
			TimeSpan duration = durationShare * parent.Duration;
			long memory = (long)(memoryShare * parent.MemoryUsed);

			PerformanceResult estimate = new(system, user, duration, memory);
			breakdowns.Add(pair.Key, pair.Value);
		}

		return breakdowns;
	}
	#endregion

	#region Helpers
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static double Correct(double value) => double.IsNaN(value) ? 0 : value;
	#endregion
}

public static class IPerformanceResultExtensions
{
	extension(IEnumerable<IPerformanceResult> results)
	{
		#region Methods
		public IPerformanceResult Sum()
		{
			TimeSpan systemTime = default, userTime = default, duration = default;
			long memoryUsed = default;

			foreach (IPerformanceResult result in results)
			{
				systemTime += result.SystemTime;
				userTime += result.UserTime;
				duration += result.Duration;
				memoryUsed += result.MemoryUsed;
			}

			return new PerformanceResult(systemTime, userTime, duration, memoryUsed);
		}
		#endregion
	}

	extension(IPerformanceResult result)
	{
		#region Properties
		public string MemoryUsedFormatted => AsMemory(result.MemoryUsed);
		#endregion

		#region Methods
		public IReadOnlyDictionary<string, IPerformanceResult> CalculateStageBreakdown(IEnumerable<IStageResultPerformance> results)
		{
			return PerformanceResult.CalculateStageBreakdown(result, results);
		}
		#endregion

		#region Helpers
		private static string AsMemory(long bytes)
		{
			ReadOnlySpan<string> suffixes = ["B", "KiB", "MiB", "GiB"];


			int i = 0;
			double value = bytes;
			while (value > 1024 && i < suffixes.Length - 1)
			{
				value /= 1024;
				i++;
			}

			return i is 0 ? $"{bytes} B" : $"{value:n2} {suffixes[i]}";
		}
		#endregion
	}
}
