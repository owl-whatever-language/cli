namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions;

public interface IFunctionReturn
{
	#region Properties
	IType Type { get; }
	ICallableFunctionReturn AsCallable { get; }
	#endregion
}
