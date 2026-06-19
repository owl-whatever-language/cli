namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols;

public interface ISymbolGroup : IReadOnlyCollection<ISymbol>
{
}

public sealed class SymbolGroup : List<ISymbol>, ISymbolGroup { }
