namespace OwlDomain.ParsingTools.Results;

public interface IStageResult
{
	#region Properties
	string Stage { get; }
	#endregion
}

public interface ISourceStageResult : IStageResult
{
	#region Properties
	ISourceFile Source { get; }
	#endregion
}

public interface IStageResultDiagnostics : IStageResult
{
	#region Properties
	IDiagnosticBag Diagnostics { get; }
	#endregion
}

public interface IStageResultPerformance : IStageResult
{
	#region Properties
	IPerformanceResult Performance { get; }
	#endregion
}

public interface IStageResultParent : IStageResult
{
	#region Properties
	IReadOnlyCollection<IStageResult> Children { get; }
	#endregion
}

public interface IStageResultParent<out T> : IStageResultParent
	where T : class, IStageResult
{
	#region Properties
	new IReadOnlyCollection<T> Children { get; }
	IReadOnlyCollection<IStageResult> IStageResultParent.Children => Children;
	#endregion
}

public interface IOrderedStageResultParent : IStageResultParent
{
	#region Properties
	new IReadOnlyList<IStageResult> Children { get; }
	IReadOnlyCollection<IStageResult> IStageResultParent.Children => Children;
	#endregion
}

public interface IOrderedStageResultParent<out T> : IOrderedStageResultParent, IStageResultParent<T>
	where T : class, IStageResult
{
	#region Properties
	new IReadOnlyList<T> Children { get; }
	IReadOnlyList<IStageResult> IOrderedStageResultParent.Children => Children;
	IReadOnlyCollection<T> IStageResultParent<T>.Children => Children;
	#endregion
}

public interface ICombinedStageResult : IOrderedStageResultParent
{
}

public interface ICombinedStageResult<out T> : ICombinedStageResult, IOrderedStageResultParent<T>
	where T : class, IStageResult
{
	#region Properties
	new IReadOnlyList<T> Children { get; }
	IReadOnlyCollection<IStageResult> IStageResultParent.Children => Children;
	IReadOnlyList<T> IOrderedStageResultParent<T>.Children => Children;
	IReadOnlyList<IStageResult> IOrderedStageResultParent.Children => Children;
	#endregion
}

public interface IParallelStageResult : IStageResultParent, IStageResultPerformance
{
}

public interface IParallelStageResult<out T> : IParallelStageResult, IStageResultParent<T>
	where T : class, IStageResult
{
}

public interface IStagePerformanceBreakdownResult : IStageResultPerformance
{
	#region Methods
	IReadOnlyDictionary<string, IPerformanceResult> GetStagePerformanceBreakdown();
	#endregion
}

public static class IStageResultExtensions
{
	extension(IStageResult result)
	{
		#region Methods
		private static void CollectDiagnostics(List<IDiagnostic> target, IStageResult current)
		{
			if (current is IStageResultDiagnostics diagnostics)
				target.AddRange(diagnostics.Diagnostics);

			if (current is IStageResultParent parent)
			{
				foreach (IStageResult child in parent.Children)
					CollectDiagnostics(target, child);
			}
		}
		public IDiagnosticBag GetAllDiagnostics()
		{
			List<IDiagnostic> target = [];
			CollectDiagnostics(target, result);

			return new DiagnosticBag(target);
		}
		public IEnumerable<IStageResult> TryGetChildren()
		{
			if (result is IStageResultParent parent)
				return parent.Children;

			return [];
		}
		#endregion
	}

	extension(IEnumerable<IStageResult> results)
	{
		public IDiagnosticBag GetAllDiagnostics()
		{
			List<IDiagnostic> target = [];

			foreach (IStageResult current in results)
				CollectDiagnostics(target, current);

			return new DiagnosticBag(target);
		}
	}

	extension<T>(IStageResultParent<T> result) where T : class, ICombinedStageResult, ISourceStageResult
	{
		#region Methods
		public IReadOnlyDictionary<ISourceFile, T> GetByFile() => result.Children.ToDictionary(r => r.Source);
		#endregion
	}
}
