namespace OwlDomain.Owl.Code.Execution.Builtins;

public interface IBuiltinResolutionStageResult : IStageResultPerformance
{
	#region Properties
	ISymbolScope ResultScope { get; }
	#endregion
}

internal sealed class BuiltinCoreResolutionResult : IBuiltinResolutionStageResult
{
	#region Properties
	public string Stage => "builtin_core_resolution";
	public IPerformanceResult Performance { get; }
	public ISymbolScope ResultScope { get; }
	public BuiltinContext Context { get; }
	#endregion

	#region Constructors
	public BuiltinCoreResolutionResult(
		IPerformanceResult performance,
		ISymbolScope resultScope,
		BuiltinContext context)
	{
		Performance = performance;
		ResultScope = resultScope;
		Context = context;
	}
	#endregion
}

public sealed class BuiltinStandardResolutionResult : IBuiltinResolutionStageResult
{
	#region Properties
	public string Stage => "builtin_standard_resolution";
	public IPerformanceResult Performance { get; }
	public ISymbolScope ResultScope { get; }
	#endregion

	#region Constructors
	public BuiltinStandardResolutionResult(IPerformanceResult performance, ISymbolScope resultScope)
	{
		Performance = performance;
		ResultScope = resultScope;
	}
	#endregion
}

public sealed class BuiltinResolutionResult : IStageResultPerformance, ICombinedStageResult<IBuiltinResolutionStageResult>
{
	#region Properties
	public string Stage => "builtin_resolution";
	public IPerformanceResult Performance { get; }
	public ISymbolScope ResultScope { get; }
	public IReadOnlyList<IBuiltinResolutionStageResult> Children { get; }
	#endregion

	#region Constructors
	public BuiltinResolutionResult(IPerformanceResult performance, params IReadOnlyList<IBuiltinResolutionStageResult> children)
	{
		Performance = performance;
		Children = children;
		ResultScope = children.Last().ResultScope;
	}
	#endregion
}

public static class BuiltinResolver
{
	#region Functions
	private static BuiltinCoreResolutionResult ResolveCore()
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			CoreSymbolScope core = new();

			BuiltinContext context = new(core);
			context.ResolveCore();

			return new(performance, core, context);
		}
	}
	private static BuiltinStandardResolutionResult ResolveStandard(ICoreSymbolScope core, BuiltinContext context)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			SymbolScope standard = new("standard", core);
			context.TargetScope = standard;

			context.ResolveStandard();

			return new(performance, standard);
		}
	}
	public static BuiltinResolutionResult Resolve()
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			BuiltinCoreResolutionResult core = ResolveCore();
			BuiltinStandardResolutionResult standard = ResolveStandard(core.ResultScope.Core, core.Context);

			return new(performance, core, standard);
		}
	}
	#endregion
}
