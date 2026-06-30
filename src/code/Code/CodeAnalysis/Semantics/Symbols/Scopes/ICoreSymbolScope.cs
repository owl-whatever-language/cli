namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols.Scopes;

public interface ICoreSymbolScope : ISymbolScope
{
	#region Properties
	INamedType? Text { get; }
	#endregion
}

public sealed class CoreSymbolScope : SymbolScope, ICoreSymbolScope
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private INamedType? _text;
	#endregion

	#region Properties
	public INamedType? Text => _text ??= GetCoreType("text");
	public override ICoreSymbolScope Core => this;
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
