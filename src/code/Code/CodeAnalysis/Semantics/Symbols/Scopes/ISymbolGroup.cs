namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols.Scopes;

public interface ISymbolGroup : IReadOnlyList<ISymbol>
{
}

public sealed class SymbolGroup : List<ISymbol>, ISymbolGroup
{
	#region Constructors
	public SymbolGroup() { }
	public SymbolGroup(params IEnumerable<ISymbol> symbols) : base(symbols) { }
	#endregion
}

public static class ISymbolGroupExtensions
{
	extension(IEnumerable<ISymbol> symbols)
	{
		#region Methods
		public SymbolGroup ToGroup() => [.. symbols];
		#endregion
	}
}
