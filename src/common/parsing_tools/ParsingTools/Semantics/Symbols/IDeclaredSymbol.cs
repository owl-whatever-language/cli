namespace OwlDomain.ParsingTools.Semantics.Symbols;

public interface IDeclaredSymbol : ISymbol
{
	#region Properties
	ISyntaxNode Declaration { get; }
	#endregion
}

public interface IDeclaredSymbol<T> : IDeclaredSymbol
	where T : notnull, ISyntaxNode
{
	#region Properties
	new T Declaration { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	ISyntaxNode IDeclaredSymbol.Declaration => Declaration;
	#endregion
}

public delegate void DeclaredSymbolShadowedHandler(IMutableDeclaredSymbol symbol, ISyntaxNode oldDeclaration, ISyntaxNode newDeclaration);

public interface IMutableDeclaredSymbol : IDeclaredSymbol
{
	#region Properties
	new ISyntaxNode Declaration { get; set; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	ISyntaxNode IDeclaredSymbol.Declaration => Declaration;
	#endregion

	#region Events
	event DeclaredSymbolShadowedHandler? Shadowed;
	#endregion
}

public interface IMutableDeclaredSymbol<T> : IDeclaredSymbol<T>, IMutableDeclaredSymbol
	where T : notnull, ISyntaxNode
{
	#region Properties
	new T Declaration { get; set; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	ISyntaxNode IDeclaredSymbol.Declaration => Declaration;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	T IDeclaredSymbol<T>.Declaration => Declaration;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	ISyntaxNode IMutableDeclaredSymbol.Declaration
	{
		get => Declaration;
		set => Declaration = (T)value;
	}
	#endregion
}

public abstract class BaseDeclaredSymbol<T> : BaseSymbol, IMutableDeclaredSymbol<T>
	where T : notnull, ISyntaxNode
{
	#region Properties
	public T Declaration
	{
		get;
		set
		{
			if (field is null)
			{
				field = value;
				return;
			}

			T old = field;
			field.ThrowIfInvalidShadow(value);

			field = value;
			Shadowed?.Invoke(this, old, value);
		}
	}
	#endregion

	#region Events
	public event DeclaredSymbolShadowedHandler? Shadowed;
	#endregion

	#region Constructors
	protected BaseDeclaredSymbol(T declaration)
	{
		Declaration = declaration;
	}
	#endregion
}
