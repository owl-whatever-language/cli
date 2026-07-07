namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols;

public sealed class NotFoundSymbol : ISymbol
{
	#region Properties
	public static NotFoundSymbol Instance { get; } = new();
	public string Name => "not_found";
	#endregion

	#region Constructors
	private NotFoundSymbol() { }
	#endregion

	#region Methods
	public TextFragmentCollection GetDebugText() => [new("not_found", ClassificationKind.Error)];
	#endregion
}
