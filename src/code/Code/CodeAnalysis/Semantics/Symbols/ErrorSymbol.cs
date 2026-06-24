namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols;

public sealed class ErrorSymbol : ISymbol
{
	#region Properties
	public static ErrorSymbol Instance { get; } = new();
	public string Name => "error";
	#endregion

	#region Constructors
	private ErrorSymbol() { }
	#endregion
}
