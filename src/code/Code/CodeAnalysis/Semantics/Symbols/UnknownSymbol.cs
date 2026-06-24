namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols;

public sealed class UnknownSymbol : ISymbol
{
	#region Properties
	public static UnknownSymbol Instance { get; } = new();
	public string Name => "unknown";
	#endregion

	#region Constructors
	private UnknownSymbol() { }
	#endregion
}
