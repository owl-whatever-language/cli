namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols.Scopes;

public interface ISymbolGroup : IReadOnlyList<ISymbol>
{
}

public sealed class SymbolGroup : List<ISymbol>, ISymbolGroup { }