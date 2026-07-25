namespace OwlDomain.ParsingTools.Semantics.Symbols;

public interface ISymbol : IDebugTextFactory
{
	#region Properties
	string Id { get; }
	string? Name { get; }
	ClassificationKind Classification { get; }
	#endregion
}

public abstract class BaseSymbol : ISymbol
{
	#region Properties
	public virtual string Id { get; } = Symbol.NewId();
	public abstract string? Name { get; }
	public abstract ClassificationKind Classification { get; }
	#endregion

	#region Methods
	public abstract TextFragmentCollection GetDebugText();
	#endregion
}

public static class Symbol
{
	#region Properties
	public static ISymbol Unknown => UnknownSymbol.Instance;
	#endregion

	#region Functions
	public static string NewId() => Guid.NewGuid().ToString("N");
	#endregion

	extension(ISymbol symbol)
	{
		#region Properties
		public bool IsKnown => symbol != Unknown;
		#endregion
	}

	extension(IReadOnlyCollection<ISymbol> symbols)
	{
		#region Methods
		public ClassificationKind? GetSharedClassification() => symbols.Select(s => s.Classification).Distinct().SingleOrDefault();
		#endregion
	}

	extension<T>(IReadOnlyCollection<T> symbols) where T : notnull, ISymbol
	{
		#region Methods
		public ClassificationKind? GetSharedClassification() => symbols.Select(s => s.Classification).Distinct().SingleOrDefault();
		#endregion
	}
}
