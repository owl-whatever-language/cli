namespace OwlDomain.ParsingTools.Semantics.Symbols;

public readonly struct SymbolSearchResult
{
	#region Properties
	public readonly ISymbolCollection All { get; }
	public readonly ISymbolCollection FromFirst { get; }
	public readonly ISymbolCollection FromCurrent { get; }
	public readonly ISymbolCollection FromParents { get; }
	#endregion

	#region Constructors
	public SymbolSearchResult(
		ISymbolCollection all,
		ISymbolCollection fromFirst,
		ISymbolCollection fromCurrent,
		ISymbolCollection fromParents)
	{
		All = all;
		FromFirst = fromFirst;
		FromCurrent = fromCurrent;
		FromParents = fromParents;
	}
	#endregion
}
