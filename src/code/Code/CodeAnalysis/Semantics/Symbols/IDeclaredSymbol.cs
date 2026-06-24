namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols;

public interface IDeclaredSymbol : ISymbol
{
	#region Properties
	new string? Name { get; }

	/// <summary>The syntax node that declared the symbol.</summary>
	/// <exception cref="ArgumentException">Thrown if trying to update the declaration with a node that isn't of the same kind, or isn't of a higher level.</exception>
	ISyntaxNode Declaration { get; set; }
	#endregion
}

public interface IDeclaredSymbol<T> : IDeclaredSymbol
	where T : notnull, ISyntaxNode
{
	#region Properties
	new T Declaration { get; set; }
	ISyntaxNode IDeclaredSymbol.Declaration
	{
		get => Declaration;
		set => Declaration = (T)value;
	}
	#endregion
}
