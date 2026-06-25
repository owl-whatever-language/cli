using System.Reflection;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Builtins.Core;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Builtins.Standard;

namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Builtins;

public interface IBuiltinResolutionStageResult : IStageResultPerformance
{
	#region Properties
	ISymbolScope ResultScope { get; }
	#endregion
}

public sealed class BuiltinCoreResolutionResult : IBuiltinResolutionStageResult
{
	#region Properties
	public string Stage => "builtin_core_resolution";
	public IPerformanceResult Performance { get; }
	public ISymbolScope ResultScope { get; }
	#endregion

	#region Constructors
	public BuiltinCoreResolutionResult(IPerformanceResult performance, ISymbolScope resultScope)
	{
		Performance = performance;
		ResultScope = resultScope;
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

public sealed class BuiltinResolver
{
	#region Nested types
	private readonly struct Scope(BuiltinResolver resolver) : IDisposable
	{
		#region Methods
		public void Dispose() => resolver.ExitScope();
		#endregion
	}
	#endregion

	#region Properties
	private SymbolScope TargetScope { get; }
	private Stack<SymbolScope> Scopes { get; } = [];
	private SymbolScope CurrentScope { get; set; }
	#endregion

	#region Constructors
	private BuiltinResolver(SymbolScope targetScope)
	{
		TargetScope = targetScope;
		CurrentScope = targetScope;
	}
	#endregion

	#region Functions
	private static BuiltinCoreResolutionResult ResolveCore()
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			CoreSymbolScope core = new();
			BuiltinType text = new("text");

			core.Add(text);
			core.Text = text;

			BuiltinResolver resolver = new(core);
			resolver.Resolve(typeof(CoreBuiltins));

			return new(performance, core);
		}
	}
	private static BuiltinStandardResolutionResult ResolveStandard(ICoreSymbolScope core)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			SymbolScope standard = new("standard", core);
			IType textType = core.Text ?? ThrowHelper.ThrowInvalidDataException<IType>("The text type is required for the current builtins.");

			standard.Add(new BuiltinFunction("print", [new(0, textType, "text")], new(SpecialTypes.Void)));

			BuiltinResolver resolver = new(standard);
			resolver.Resolve(typeof(StandardBuiltins));

			return new(performance, standard);
		}
	}
	public static BuiltinResolutionResult Resolve()
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			BuiltinCoreResolutionResult core = ResolveCore();
			BuiltinStandardResolutionResult standard = ResolveStandard(core.ResultScope.Core);

			return new(performance, core, standard);
		}
	}
	#endregion

	#region Methods
	private void Resolve(Type container)
	{

	}
	private BuiltinFunction ResolveFunction(MethodInfo method)
	{
		string name = ConvertName(method.Name);

		List<BuiltinFunctionParameter> parameters = [];

		foreach (ParameterInfo param in method.GetParameters())
		{
			if (parameters.Count is 0)
			{
				if (param.ParameterType.GetInterfaces().Any(i => i == typeof(IExecutionContext)))
					continue;

				ThrowHelper.ThrowInvalidOperationException($"The method ({method}) didn't have an {nameof(IExecutionContext)} as it's first parameter.");
			}
		}

		throw new NotImplementedException();
	}
	#endregion

	#region Scope helpers
	private void ExitScope()
	{
		if (Scopes.TryPop(out SymbolScope? scope))
			CurrentScope = scope;
		else
			ThrowHelper.ThrowInvalidOperationException("Exiting the target scope is not allowed.");
	}
	#endregion

	#region Helpers
	private string ConvertName(string name) => name.ToLower();
	private IType GetType(string name)
	{
		if (CurrentScope.TryGet(name, out ISymbolGroup? symbols) is false)
			ThrowHelper.ThrowInvalidOperationException($"Couldn't find any symbol named '{name}'.");

		IType[] types = symbols.OfType<IType>().ToArray();
		if (types.Length is 0)
			ThrowHelper.ThrowInvalidOperationException($"Could find some symbols named '{name}', but none of them were a type.");
		else if (types.Length > 1)
			ThrowHelper.ThrowInvalidOperationException($"Could find several types named '{name}', but they were ambiguous.");

		return types[0];
	}
	#endregion
}
