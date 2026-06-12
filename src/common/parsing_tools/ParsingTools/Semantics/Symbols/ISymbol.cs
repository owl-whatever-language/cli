namespace OwlDomain.ParsingTools.Semantics.Symbols;

/// <summary>
/// 	Represents a symbol.
/// </summary>
public interface ISymbol
{
	#region Properties
	/// <summary>The name of the symbol.</summary>
	string? Name { get; }

	/// <summary>The abstract syntax node that declared the symbol.</summary>
	IConcreteSyntaxNode? Declaration { get; }

	/// <summary>The thing that the symbol is referencing.</summary>
	ISymbolTarget Target { get; }
	#endregion
}

/// <summary>
/// 	Represents a symbol.
/// </summary>
/// <typeparam name="T">The type of the abstract syntax node that declared the symbol.</typeparam>
public interface ISymbol<T> : ISymbol
where T : notnull, IConcreteSyntaxNode
{
	#region Properties
	/// <summary>The abstract syntax node that declared the symbol.</summary>
	new T? Declaration { get; }
	IConcreteSyntaxNode? ISymbol.Declaration => Declaration;
	#endregion
}

/// <summary>
/// 	Represents a symbol.
/// </summary>
/// <typeparam name="T">The type of the abstract syntax node that declared the symbol.</typeparam>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public class Symbol<T> : ISymbol<T>
	where T : notnull, IConcreteSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public string? Name { get; }

	/// <inheritdoc/>
	public T? Declaration { get; }

	/// <inheritdoc/>
	public ISymbolTarget Target { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="Symbol{T}"/> instance.</summary>
	/// <param name="name">The name of the symbol.</param>
	/// <param name="declaration">The abstract syntax node that declared the symbol.</param>
	/// <param name="target">The thing that the symbol is referencing.</param>
	/// <exception cref="ArgumentException">Thrown if both the <paramref name="name"/> and <paramref name="declaration"/> are <see langword="null"/>.</exception>
	public Symbol(string? name, T? declaration, ISymbolTarget target)
	{
		if (name is null && declaration is null)
			ThrowHelper.ThrowArgumentException(nameof(name), "The name can only be null if the declaration is provided.");

		Name = name;
		Declaration = declaration;
		Target = target;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override string ToString() => Name ?? Target.ToString() ?? "???";
	private string DebuggerDisplay() => $"{GetType().Name} {{ Name = ({Name}), Target = ({Target}) }}";
	#endregion
}

/// <summary>
/// 	Represents a symbol that won't have a declaration.
/// </summary>
public class Symbol : Symbol<IConcreteSyntaxNode>
{
	#region Constructors
	/// <summary>Creates a new <see cref="Symbol"/> instance.</summary>
	/// <param name="name">The name of the symbol.</param>
	/// <param name="target">The thing that the symbol is referencing.</param>
	public Symbol(string name, ISymbolTarget target) : base(name, null, target) { }
	#endregion
}
