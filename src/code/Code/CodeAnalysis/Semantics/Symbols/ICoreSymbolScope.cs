namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols;

public interface ICoreSymbolScope : ISymbolScope
{
	#region Properties
	INamedType? Bool { get; }
	INamedType? Text { get; }
	INamedType? Int { get; }
	INamedType? Num { get; }
	#endregion
}

public sealed class CoreSymbolScope : SymbolScope, ICoreSymbolScope
{
	#region Properties
	public INamedType? Bool => field ??= GetCoreType("bool");
	public INamedType? Text => field ??= GetCoreType("text");
	public INamedType? Int => field ??= GetCoreType("int");
	public INamedType? Num => field ??= GetCoreType("num");
	#endregion

	#region Constructors
	public CoreSymbolScope() : base("core") { }
	#endregion

	#region Helpers
	private INamedType? GetCoreType(string name)
	{
		SymbolSearchResult symbols = Search(name);

		INamedType[]? types = symbols.All.OfType<INamedType>().ToArray();
		if (types.Length is 1)
			return types[0];

		return null;
	}
	#endregion
}
