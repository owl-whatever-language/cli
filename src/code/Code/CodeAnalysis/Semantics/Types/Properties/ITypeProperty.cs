namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Properties;

public interface ITypeProperty : ISymbol
{
	#region Properties
	IType DeclaringType { get; }
	IType Type { get; }
	new string? Name { get; }
	#endregion
}
