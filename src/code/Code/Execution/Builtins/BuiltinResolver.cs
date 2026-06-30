using System.Reflection;
using OwlDomain.Owl.Code.Execution.Builtins.Attributes;
using OwlDomain.Owl.Code.Execution.Builtins.Core;
using OwlDomain.Owl.Code.Execution.Builtins.Standard;

namespace OwlDomain.Owl.Code.Execution.Builtins;

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
	public IReadOnlyDictionary<Type, IType> TypeLookup { get; }
	#endregion

	#region Constructors
	public BuiltinCoreResolutionResult(
		IPerformanceResult performance,
		ISymbolScope resultScope,
		IReadOnlyDictionary<Type, IType> typeLookup)
	{
		Performance = performance;
		ResultScope = resultScope;
		TypeLookup = typeLookup;
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
	private Dictionary<Type, IType> TypeLookup { get; }
	#endregion

	#region Constructors
	private BuiltinResolver(SymbolScope targetScope, IReadOnlyDictionary<Type, IType>? typeLookup)
	{
		TargetScope = targetScope;
		CurrentScope = targetScope;
		TypeLookup = typeLookup is null ? [] : new(typeLookup);
	}
	#endregion

	#region Functions
	private static BuiltinCoreResolutionResult ResolveCore()
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			CoreSymbolScope core = new();

			BuiltinResolver resolver = new(core, null);
			resolver.Resolve(typeof(CoreBuiltins));

			return new(performance, core, resolver.TypeLookup);
		}
	}
	private static BuiltinStandardResolutionResult ResolveStandard(ICoreSymbolScope core, IReadOnlyDictionary<Type, IType> typeLookup)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			SymbolScope standard = new("standard", core);

			BuiltinResolver resolver = new(standard, typeLookup);
			resolver.Resolve(typeof(StandardBuiltins));

			return new(performance, standard);
		}
	}
	public static BuiltinResolutionResult Resolve()
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			BuiltinCoreResolutionResult core = ResolveCore();
			BuiltinStandardResolutionResult standard = ResolveStandard(core.ResultScope.Core, core.TypeLookup);

			return new(performance, core, standard);
		}
	}
	#endregion

	#region Methods
	private void Resolve(Type container)
	{
		Type[] types = container.GetNestedTypes();

		foreach (Type type in types)
		{
			if (ShouldIgnore(type))
				continue;

			string name = GetName(type);
			BuiltinType builtinType = new(name, type);

			TypeLookup.Add(type, builtinType);
			CurrentScope.Add(builtinType);
		}

		MethodInfo[] functions = container.GetMethods(BindingFlags.Static | BindingFlags.Public);
		foreach (MethodInfo function in functions)
		{
			if (ShouldIgnore(function))
				continue;

			ResolveFunction(function);
		}
	}
	private void ResolveFunction(MethodInfo function)
	{
		string name = GetName(function);

		ParameterInfo[] parameters = function.GetParameters();
		if (parameters.Length is 0 || (parameters[0].ParameterType != typeof(IExecutionContext)))
			ThrowHelper.ThrowInvalidOperationException($"Expected the builtin function '{name}' to have a first parameter of the type '{typeof(IExecutionContext)}'.");

		List<BuiltinFunctionParameter> builtinParameters = [];
		for (int i = 1; i < parameters.Length; i++)
		{
			ParameterInfo parameter = parameters[i];

			string? parameterName = parameter.GetCustomAttribute<NameAttribute>()?.Name ?? parameter.Name;
			if (parameterName is null)
				ThrowHelper.ThrowInvalidOperationException($"Expected the parameter '{parameter}' on '{function}' to have a name.");

			IType type = TypeLookup[parameter.ParameterType];
			BuiltinFunctionParameter builtinParameter = new(i, type, parameterName);
			builtinParameters.Add(builtinParameter);
		}

		IType returnType = function.ReturnType == typeof(void) ? SpecialTypes.Void : TypeLookup[function.ReturnType];
		BuiltinFunctionReturn @return = new(returnType);

		BuiltinFunction builtin = new(name, function, builtinParameters, @return);
		CurrentScope.Add(builtin);
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

	#region Attribute helpers
	private static bool ShouldIgnore(MemberInfo member)
	{
		return member.GetCustomAttribute<IgnoreAttribute>() is not null;
	}
	private static string GetName(MemberInfo member)
	{
		return member.GetCustomAttribute<NameAttribute>()?.Name ?? member.Name;
	}
	#endregion

	#region Helpers
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
