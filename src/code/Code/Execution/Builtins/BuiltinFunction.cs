namespace OwlDomain.Owl.Code.Execution.Builtins;

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
internal sealed class BuiltinFunction : BaseSymbol, IFunction
{
	#region Nested types
	public delegate InterpreterValue ExecuteDelegate(IExecutionContext context, IReadOnlyList<InterpreterValue> values);
	#endregion

	#region Properties
	public override string Name { get; }
	public override ClassificationKind Classification => ClassificationKind.Function;
	public IReadOnlyList<IFunctionParameter> Parameters { get; }
	public IFunctionReturn Return { get; }
	public ICallableFunction AsCallable { get; }
	public ExecuteDelegate ExecuteCallback { get; }
	#endregion

	#region Constructors
	public BuiltinFunction(string name, IReadOnlyList<BuiltinFunctionParameter> parameters, BuiltinFunctionReturn @return, ExecuteDelegate execute)
	{
		char first = name.First();
		if (first == char.ToUpper(first) && Enum.TryParse<OperatorKind>(name, out _) is false)
			ThrowHelper.ThrowInvalidOperationException($"Functions should be camelCase, but instead got ({name}).");

		Name = name;
		Parameters = parameters;
		Return = @return;
		ExecuteCallback = execute;

		AsCallable = new CallableFunction(this);
	}
	#endregion

	#region Methods
	public InterpreterValue Execute(IExecutionContext context, IReadOnlyList<InterpreterValue> values)
	{
		InterpreterValue result = ExecuteCallback.Invoke(context, values);
		return result;
	}

	public override TextFragmentCollection GetDebugText()
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

	#region Helpers
	private string DebuggerDisplay() => $"{nameof(BuiltinFunction)} {{ Name = ({Name}) }}";
	#endregion
}

internal sealed class BuiltinFunctionParameter : BaseSymbol, IFunctionParameter
{
	#region Properties
	public override string Name { get; }
	public override ClassificationKind Classification => ClassificationKind.Parameter;
	public int Index { get; }
	public IType Type { get; }
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
	public override TextFragmentCollection GetDebugText()
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
