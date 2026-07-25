namespace OwlDomain.ParsingTools.Semantics.Symbols;

public sealed class UnknownSymbol : BaseSymbol
{
	#region Properties
	public static UnknownSymbol Instance { get; } = new();
	public override string Id { get; } = Guid.Empty.ToString("N");
	public override string Name => "unknown";
	public override ClassificationKind Classification => ClassificationKind.Error;
	#endregion

	#region Constructors
	private UnknownSymbol() { }
	#endregion

	#region Methods
	public override TextFragmentCollection GetDebugText() => [new(Name, Classification)];
	#endregion
}
