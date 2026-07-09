using System.Reflection;

namespace OwlDomain.Owl.Code.Execution.Builtins;

internal sealed class BuiltinFunction : IFunction
{
	#region Properties
	public string Name { get; }
	public MethodInfo Method { get; }
	public bool TakesContext { get; }
	public IReadOnlyList<IFunctionParameter> Parameters { get; }
	public IFunctionReturn Return { get; }
	public ICallableFunction AsCallable { get; }
	#endregion

	#region Constructors
	public BuiltinFunction(string name, MethodInfo method, IReadOnlyList<BuiltinFunctionParameter> parameters, BuiltinFunctionReturn @return)
	{
		Name = name;
		Method = method;
		TakesContext = method.GetParameters().FirstOrDefault()?.ParameterType == typeof(IExecutionContext);

		Parameters = parameters;
		Return = @return;

		AsCallable = new CallableFunction(this);
	}
	#endregion

	#region Methods
	public InterpreterValue Execute(IExecutionContext context, IReadOnlyList<InterpreterValue> values)
	{
		object?[] withContext = TakesContext ? [context] : [];

		object?[] arguments =
		[
			..withContext,
			.. values.Select(v => v.Value)
		];

		object? result = Method.Invoke(null, arguments);

		return Return.Type == SpecialTypes.Void ? InterpreterValue.Void : new(Return.Type, result);
	}

	public TextFragmentCollection GetDebugText()
	{
		TextFragmentCollection fragments = [];

		fragments.Add(Name, ClassificationKind.Function);
		fragments.Add("(", ClassificationKind.Punctuation);

		for (int i = 0; i < Parameters.Count; i++)
		{
			if (i > 0)
				fragments.Add(", ", ClassificationKind.Punctuation);

			fragments.AddRange(Parameters[i]);
		}

		fragments.Add(")", ClassificationKind.Punctuation);

		if (Return.Type != SpecialTypes.Void)
		{
			fragments.Add(": ", ClassificationKind.Punctuation);
			fragments.AddRange(Return.Type);
		}

		return fragments;
	}
	#endregion
}

internal sealed class BuiltinFunctionParameter : IFunctionParameter
{
	#region Properties
	public int Index { get; }
	public IType Type { get; }
	public string Name { get; }
	public ICallableFunctionParameter AsCallable { get; }
	#endregion

	#region Constructors
	public BuiltinFunctionParameter(int index, IType type, string name)
	{
		Index = index;
		Type = type;
		Name = name;
		AsCallable = new CallableFunctionParameter(this);
	}
	#endregion

	#region Methods
	public TextFragmentCollection GetDebugText()
	{
		TextFragmentCollection fragments = [];

		fragments.AddRange(Type);
		fragments.Add(" ", ClassificationKind.Whitespace);
		fragments.Add(Name, ClassificationKind.Parameter);

		return fragments;
	}
	#endregion
}

internal sealed class BuiltinFunctionReturn : IFunctionReturn
{
	#region Properties
	public IType Type { get; }
	public ICallableFunctionReturn AsCallable { get; }
	#endregion

	#region Constructors
	public BuiltinFunctionReturn(IType type)
	{
		Type = type;
		AsCallable = new CallableFunctionReturn(this);
	}
	#endregion
}
