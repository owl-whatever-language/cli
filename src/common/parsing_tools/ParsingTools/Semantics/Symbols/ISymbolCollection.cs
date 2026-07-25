namespace OwlDomain.ParsingTools.Semantics.Symbols;

public interface ISymbolCollection : IReadOnlyCollection<ISymbol>
{
}

public interface IMutableSymbolCollection : ISymbolCollection, ICollection<ISymbol>
{
}

public sealed class SymbolCollection : List<ISymbol>, IMutableSymbolCollection
{
	#region Properties
	public static ISymbolCollection Empty { get; } = new SymbolCollection();
	#endregion

	#region Constructors
	public SymbolCollection() { }
	public SymbolCollection(params IEnumerable<ISymbol> symbols) : base(symbols) { }
	#endregion
}

public static class SymbolCollectionExtensions
{
	extension(IEnumerable<ISymbol> symbols)
	{
		#region Methods
		public IMutableSymbolCollection ToCollection() => new SymbolCollection(symbols);
		#endregion
	}
	extension<T>(IEnumerable<T> symbols) where T : notnull, ISymbol
	{
		#region Methods
		public IMutableSymbolCollection ToCollection() => new SymbolCollection(symbols.Cast<ISymbol>());
		#endregion
	}

	extension(IReadOnlyCollection<ISymbol> symbols)
	{
		#region Methods
		public ISymbolCollection GetAlternative(string? name)
		{
			SymbolCollection alternative = [];

			if (name is null)
				return alternative;

			ISymbol? lowest = null;
			int score = int.MaxValue;

			string upper = name.ToUpperInvariant();

			foreach (ISymbol symbol in symbols)
			{
				if (symbol.Name is null)
					continue;

				if (symbol.Name.ToUpperInvariant() == upper)
				{
					alternative.Add(symbol);
					return alternative;
				}

				int current = GetDistance(name, symbol.Name, out int max);
				if (current > 2 && current > max / 2)
					continue;

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
	}

	#region Helpers
	private static int GetDistance(string from, string to, out int max)
	{
		const int deleteCost = 10;
		const int insertCost = 10;
		const int substituteCost = 10;
		const int caseCost = 5;

		static int GetSubstituteCost(char from, char to)
		{
			if (from == to)
				return 0;

			if (char.ToUpperInvariant(from) == char.ToUpperInvariant(to))
				return caseCost;

			return substituteCost;
		}

		int[] v0 = new int[to.Length + 1];
		int[] v1 = new int[to.Length + 1];

		for (int i = 0; i <= to.Length; i++)
			v0[i] = i;

		for (int i = 0; i < from.Length; i++)
		{
			v1[0] = i + 1;

			for (int j = 0; j < to.Length; j++)
			{
				int delete = v0[j + 1] + deleteCost;
				int insert = v1[j] + insertCost;
				int substitute = v0[j] + GetSubstituteCost(from[i], to[j]);

				int min = Math.Min(delete, Math.Min(insert, substitute));
				v1[j + 1] = min;
			}

			(v0, v1) = (v1, v0);
		}

		max = v0.Concat(v1).Max();
		return v0[to.Length];
	}
	#endregion
}
