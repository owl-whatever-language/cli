namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions.Callable;

public interface ICallableFunctionParameter : ICallableTypeParameter
{
	#region Properties
	IFunctionParameter FunctionParameter { get; }
	#endregion
}

public sealed class CallableFunctionParameter : ICallableFunctionParameter
{
	#region Properties
	public IFunctionParameter FunctionParameter { get; }
	public int Index => FunctionParameter.Index;
	public string? Name => FunctionParameter.Name;
	public IType Type => FunctionParameter.Type;
	#endregion

	#region Constructor
	public CallableFunctionParameter(IFunctionParameter parameter) => FunctionParameter = parameter;
	#endregion

	#region Methods
	TextFragmentCollection IDebugTreePrintable.GetFragments()
	{
		TextFragmentCollection fragments = [];

		fragments.AddRange(Type);
		if (Name is not null)
		{
			fragments.Add(" ", ClassificationKind.Whitespace);
			fragments.Add(Name, ClassificationKind.Parameter);
		}

		return fragments;
	}
	#endregion
}
