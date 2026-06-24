namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions.Callable;

public interface ICallableFunctionReturn : ICallableTypeReturn
{
	#region Properties
	IFunctionReturn FunctionReturn { get; }
	#endregion
}

public sealed class CallableFunctionReturn : ICallableFunctionReturn
{
	#region Properties
	public IFunctionReturn FunctionReturn { get; }
	public IType Type => FunctionReturn.Type;
	#endregion

	#region Constructors
	public CallableFunctionReturn(IFunctionReturn @return) => FunctionReturn = @return;
	#endregion
}
