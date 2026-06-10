namespace OwlDomain.ParsingTools.Semantics;

/// <summary>
/// 	Represents a symbol.
/// </summary>
public interface ISymbol
{
	#region Properties
	/// <summary>The name of the symbol.</summary>
	string? Name { get; }

	/// <summary>The abstract syntax node that declared the symbol.</summary>
	IAbstractSyntaxNode? Declaration { get; }
	#endregion
}

/// <summary>
/// 	Represents a symbol.
/// </summary>
/// <typeparam name="T">The type of the abstract syntax node that declared the symbol.</typeparam>
public interface ISymbol<T> : ISymbol
where T : notnull, IAbstractSyntaxNode
{
	#region Properties
	/// <summary>The abstract syntax node that declared the symbol.</summary>
	new T? Declaration { get; }
	IAbstractSyntaxNode? ISymbol.Declaration => Declaration;
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a symbol.
/// </summary>
/// <typeparam name="T">The type of the abstract syntax node that declared the symbol.</typeparam>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public abstract class BaseSymbol<T> : ISymbol<T>
	where T : notnull, IAbstractSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public string? Name { get; }

	/// <inheritdoc/>
	public T? Declaration { get; }
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="BaseSymbol{T}"/> properties.</summary>
	/// <param name="name">The name of the symbol.</param>
	/// <param name="declaration">The abstract syntax node that declared the symbol.</param>
	protected BaseSymbol(string? name, T? declaration)
	{
		Name = name;
		Declaration = declaration;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override string ToString() => Name ?? "???";
	private string DebuggerDisplay() => $"{GetType().Name} {{ Name = ({Name}) }}";
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a symbol that won't have a declaration.
/// </summary>
public abstract class BaseSymbol : BaseSymbol<IAbstractSyntaxNode>
{
	#region Constructors
	/// <summary>Populates the <see cref="BaseSymbol"/> properties.</summary>
	/// <param name="name">The name of the symbol.</param>
	protected BaseSymbol(string? name) : base(name, null) { }
	#endregion
}
