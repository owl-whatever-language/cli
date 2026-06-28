namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Builtins;

public sealed class BuiltinFunction : IFunction
{
	#region Properties
	public string Name { get; }
	public IReadOnlyList<IFunctionParameter> Parameters { get; }
	public IFunctionReturn Return { get; }
	public ICallableFunction AsCallable { get; }
	#endregion

	#region Constructors
	public BuiltinFunction(string name, IReadOnlyList<BuiltinFunctionParameter> parameters, BuiltinFunctionReturn @return)
	{
		Name = name;
		Parameters = parameters;
		Return = @return;

		AsCallable = new CallableFunction(this);
	}
	#endregion

	#region Methods
	TextFragmentCollection IDebugTreePrintable.GetFragments()
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

public sealed class BuiltinFunctionParameter : IFunctionParameter
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
	TextFragmentCollection IDebugTreePrintable.GetFragments()
	{
		TextFragmentCollection fragments = [];

		fragments.AddRange(Type);
		fragments.Add(" ", ClassificationKind.Whitespace);
		fragments.Add(Name, ClassificationKind.Parameter);

		return fragments;
	}
	#endregion
}

public sealed class BuiltinFunctionReturn : IFunctionReturn
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
