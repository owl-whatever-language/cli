namespace OwlDomain.Owl.Code.Semantics.Symbols;

public interface ISymbol
{
	#region Properties
	string? Name { get; }
	ISymbolTarget Target { get; }
	#endregion
}

public interface IDeclaredSymbol : ISymbol
{
	#region Properties
	IConcreteSyntaxNode Declaration { get; }
	#endregion
}

public sealed class Symbol : ISymbol
{
	#region Properties
	/// <inheritdoc/>
	public string Name { get; }

	/// <inheritdoc/>
	public ISymbolTarget Target { get; }
	#endregion

	#region Constructors
	public Symbol(string name, ISymbolTarget target)
	{
		Name = name;
		Target = target;
	}
	#endregion
}

public sealed class DeclaredSymbol : IDeclaredSymbol
{
	#region Properties
	/// <inheritdoc/>
	public string? Name { get; }

	/// <inheritdoc/>
	public ISymbolTarget Target { get; }

	/// <inheritdoc/>
	public IConcreteSyntaxNode Declaration { get; }
	#endregion

	#region Constructors
	public DeclaredSymbol(string? name, ISymbolTarget target, IConcreteSyntaxNode declaration)
	{
		Name = name;
		Target = target;
		Declaration = declaration;
	}
	#endregion
}
