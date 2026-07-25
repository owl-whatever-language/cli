namespace OwlDomain.ParsingTools.Semantics.Symbols;

public interface IDeclaredSymbolScope : ISymbolScope
{
	#region Properties
	ISyntaxNode Declaration { get; }
	#endregion
}

public delegate void DeclaredSymbolScopeShadowedHandler(IMutableDeclaredSymbolScope scope, ISyntaxNode oldDeclaration, ISyntaxNode newDeclaration);

public interface IMutableDeclaredSymbolScope : IDeclaredSymbolScope, IMutableSymbolScope
{
	#region Properties
	new ISyntaxNode Declaration { get; set; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	ISyntaxNode IDeclaredSymbolScope.Declaration => Declaration;
	#endregion

	#region Events
	event DeclaredSymbolScopeShadowedHandler? Shadowed;
	#endregion
}

public sealed class DeclaredSymbolScope : SymbolScope, IMutableDeclaredSymbolScope
{
	#region Properties
	public ISyntaxNode Declaration
	{
		get;
		set
		{
			if (field is null)
			{
				field = value;
				return;
			}

			ISyntaxNode old = field;
			field.ThrowIfInvalidShadow(value);

			field = value;
			Shadowed?.Invoke(this, old, value);
		}
	}
	#endregion

	#region Methods
	public event DeclaredSymbolScopeShadowedHandler? Shadowed;
	#endregion

	#region Constructors
	public DeclaredSymbolScope(ISymbolScope parent, ISyntaxNode declaration, string name) : base(parent, name)
	{
		Declaration = declaration;
	}
	#endregion
}
