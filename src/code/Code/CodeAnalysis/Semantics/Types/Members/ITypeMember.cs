namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Members;

public interface ITypeMember : ISymbol
{
	#region Properties
	IType DeclaringType { get; }
	new string? Name { get; }

	string ISymbol.Name => Name ?? SymbolHelpers.ThrowSymbolWithoutNameException<string>();
	#endregion
}
