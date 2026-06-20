namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols;

public interface ISymbolGroup : IReadOnlyCollection<ISymbol>
{
}

public sealed class SymbolGroup : List<ISymbol>, ISymbolGroup { }

public static class ISymbolGroupExtensions
{
	extension(ISymbolGroup group)
	{
		#region Methods
		public IReadOnlyList<T> ForTarget<T>() => group.Select(s => s.Target).OfType<T>().ToArray();
		#endregion
	}
}