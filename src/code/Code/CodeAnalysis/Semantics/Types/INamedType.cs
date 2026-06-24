namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;

public interface INamedType : IType, ISymbol
{
	#region Properties
	new string? Name { get; }
	#endregion
}
