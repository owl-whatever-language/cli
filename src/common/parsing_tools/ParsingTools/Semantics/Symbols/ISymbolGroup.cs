namespace OwlDomain.ParsingTools.Semantics.Symbols;

/// <summary>
/// 	Represents a collection of symbols.
/// </summary>
public interface ISymbolGroup : IReadOnlyCollection<ISymbol>
{
}

/// <summary>
/// 	Represents a collection of symbols.
/// </summary>
public class SymbolGroup : List<ISymbol>, ISymbolGroup
{
}
