namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols.Scopes;

public interface ICoreSymbolScope : ISymbolScope
{
	#region Properties
	INamedType? Bool { get; }
	INamedType? Text { get; }
	INamedType? Int { get; }
	#endregion
}

public sealed class CoreSymbolScope : SymbolScope, ICoreSymbolScope
{
	#region Properties
	public override ICoreSymbolScope Core => this;
	public INamedType? Bool => field ??= GetCoreType("bool");
	public INamedType? Text => field ??= GetCoreType("text");
	public INamedType? Int => field ??= GetCoreType("int");
	#endregion

	#region Constructors
	public CoreSymbolScope() : base("core") { }
	#endregion

	#region Helpers
	private INamedType? GetCoreType(string name)
	{
		if (TryGet(name, out ISymbolGroup? symbols) is false)
			return null;

		INamedType[]? types = symbols.OfType<INamedType>().ToArray();
		if (types.Length is 1)
			return types[0];

		return null;
	}
	#endregion
}
