namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols.Scopes;

public interface ISymbolGroup : IReadOnlyList<ISymbol>
{
	#region Methods
	ISymbolGroup GetAlternative(string name);
	#endregion
}

public sealed class SymbolGroup : List<ISymbol>, ISymbolGroup
{
	#region Constructors
	public SymbolGroup() { }
	public SymbolGroup(params IEnumerable<ISymbol> symbols) : base(symbols) { }
	#endregion

	#region Methods
	public ISymbolGroup GetAlternative(string name)
	{
		SymbolGroup alternative = [];

		foreach (ISymbol symbol in this)
		{
			if (symbol.Name.ToUpperInvariant() == name.ToUpperInvariant())
			{
				alternative.Add(symbol);
				return alternative;
			}
		}

		ISymbol? lowest = null;
		int score = int.MaxValue;

		foreach (ISymbol symbol in this)
		{
			int current = GetDistance(name, symbol.Name);
			if (lowest is null || current < score)
			{
				lowest = symbol;
				score = current;
			}
		}

		if (lowest is not null)
			alternative.Add(lowest);

		return alternative;
	}
	#endregion

	#region Helpers
	private static int GetDistance(string from, string to)
	{
		int[] v0 = new int[to.Length + 1];
		int[] v1 = new int[to.Length + 1];

		for (int i = 0; i <= to.Length; i++)
			v0[i] = i;

		for (int i = 0; i < from.Length; i++)
		{
			v1[0] = i + 1;

			for (int j = 0; j < to.Length; j++)
			{
				int delete = v0[j + 1] + 1;
				int insert = v1[j] + 1;
				int substitute = from[i] == to[j] ? v0[j] : v0[j] + 1;

				v1[j + 1] = Math.Min(delete, Math.Min(insert, substitute));
			}

			(v0, v1) = (v1, v0);
		}

		return v0[to.Length];
	}
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

	extension<T>(IEnumerable<T> symbols) where T : notnull, ISymbol
	{
		#region Methods
		public SymbolGroup ToGroup() => [.. symbols];
		#endregion
	}
}
