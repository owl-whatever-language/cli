namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions;

public interface IFunctionParameter : ISymbol
{
	#region Properties
	int Index { get; }
	IType Type { get; }
	new string? Name { get; }
	ICallableFunctionParameter AsCallable { get; }
	#endregion
}
