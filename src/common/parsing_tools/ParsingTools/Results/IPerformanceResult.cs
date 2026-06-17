namespace OwlDomain.ParsingTools.Results;

public interface IPerformanceResult
{
	#region Properties
	TimeSpan SystemTime { get; }
	TimeSpan UserTime { get; }
	TimeSpan Duration { get; }
	long MemoryUsed { get; }
	#endregion
}

internal sealed class PerformanceResultScope : IPerformanceResult
{
	#region Fields
	private readonly TimeSpan _systemTimeAtStart;
	private readonly TimeSpan _userTimeAtStart;
	private readonly long _memoryAtStart;

	private TimeSpan? _systemTimeAtEnd;
	private TimeSpan? _userTimeAtEnd;
	private long? _memoryAtEnd;
	#endregion

	#region Properties
	/// <inheritdoc/>
	public TimeSpan SystemTime => _systemTimeAtEnd is null ? default : _systemTimeAtEnd.Value - _systemTimeAtStart;

	/// <inheritdoc/>
	public TimeSpan UserTime => _userTimeAtEnd is null ? default : _userTimeAtEnd.Value - _userTimeAtStart;

	/// <inheritdoc/>
	public TimeSpan Duration => SystemTime + UserTime;

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
	}
	#endregion

	#region Methods
	internal void Stop()
	{
		Environment.ProcessCpuUsage cpu = Environment.CpuUsage;
		_systemTimeAtEnd = cpu.PrivilegedTime;
		_userTimeAtEnd = cpu.UserTime;
		_memoryAtEnd = GC.GetTotalMemory(true);
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
}
