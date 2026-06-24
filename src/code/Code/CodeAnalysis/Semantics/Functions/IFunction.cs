namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions;

public interface IFunction : ISymbol
{
	#region Properties
	new string? Name { get; }
	IReadOnlyList<IFunctionParameter> Parameters { get; }
	IFunctionReturn Return { get; }
	ICallableFunction AsCallable { get; }
	#endregion
}
