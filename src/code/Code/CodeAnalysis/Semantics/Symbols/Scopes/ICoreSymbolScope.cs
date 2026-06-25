namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols.Scopes;

public interface ICoreSymbolScope : ISymbolScope
{
	#region Properties
	INamedType? Text { get; }
	#endregion
}

public sealed class CoreSymbolScope : SymbolScope, ICoreSymbolScope
{
	#region Properties
	public INamedType? Text { get; set; }
	public override ICoreSymbolScope Core => this;
	#endregion

	#region Constructors
	public CoreSymbolScope() : base("core") { }
	#endregion
}
