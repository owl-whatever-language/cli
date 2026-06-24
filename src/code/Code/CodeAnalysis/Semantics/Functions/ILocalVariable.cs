namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions;

public interface ILocalVariable : ISymbol
{
	#region Properties
	IType Type { get; }
	new string? Name { get; }
	#endregion
}
