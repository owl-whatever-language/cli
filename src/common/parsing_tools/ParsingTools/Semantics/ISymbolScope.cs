namespace OwlDomain.ParsingTools.Semantics;

/// <summary>
/// 	Represents a scope for symbols.
/// </summary>
public interface ISymbolScope
{
	#region Properties
	/// <summary>The name of the scope, to help with debugging.</summary>
	string Name { get; }

	/// <summary>The parent scope.</summary>
	ISymbolScope? Parent { get; }

	/// <summary>The root scope.</summary>
	ISymbolScope Root { get; }

	/// <summary>The amount of symbols in the scope.</summary>
	int Count { get; }

	/// <summary>The amount of symbols in the scope, and all the parent scopes combined.</summary>
	int TotalCount { get; }
	#endregion

	#region Methods
	/// <summary>Tries to get a collection of <paramref name="symbols"/> for the given <paramref name="name"/>.</summary>
	/// <param name="name">The name of the symbols to find.</param>
	/// <param name="symbols">The found <paramref name="symbols"/>.</param>
	/// <returns>
	/// 	<see langword="true"/> if any <paramref name="symbols"/> could be found for
	/// 	the given <paramref name="name"/>, <see langword="false"/> otherwise.
	/// </returns>
	bool TryGet(string name, [NotNullWhen(true)] out ISymbolGroup? symbols);

	/// <summary>tries to get the <paramref name="symbol"/> that was created for the given <paramref name="declaration"/>.</summary>
	/// <param name="declaration">The abstract syntax node that created the symbol.</param>
	/// <param name="symbol">The symbol that was created for the given <paramref name="declaration"/>.</param>
	/// <returns><see langword="true"/> if a <paramref name="symbol"/> could be found, <see langword="false"/> otherwise.</returns>
	bool TryGet(IAbstractSyntaxNode declaration, [NotNullWhen(true)] out ISymbol? symbol);
	#endregion
}

/// <summary>
/// 	Represents a mutable scope for symbols.
/// </summary>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class SymbolScope : ISymbolScope
{
	#region Fields
	private readonly Dictionary<string, SymbolGroup> _nameLookup = [];
	private readonly Dictionary<IAbstractSyntaxNode, ISymbol> _nodeLookup = [];
	#endregion

	#region Properties
	/// <inheritdoc/>
	public string Name { get; }

	/// <inheritdoc/>
	public ISymbolScope? Parent { get; }

	/// <inheritdoc/>
	public ISymbolScope Root => Parent?.Root ?? this;

	/// <inheritdoc/>
	public int Count { get; private set; }

	/// <inheritdoc/>
	public int TotalCount => Parent is null ? Count : Parent.Count + Count;
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="SymbolScope"/> instance.</summary>
	/// <param name="name">The name of the scope, to help with debugging.</param>
	public SymbolScope(string name) => Name = name;

	/// <summary>Creates a new <see cref="SymbolScope"/> instance.</summary>
	/// <param name="name">The name of the scope, to help with debugging.</param>
	/// <param name="parent">The parent scope.</param>
	public SymbolScope(string name, ISymbolScope parent) : this(name)
	{
		Parent = parent;
	}
	#endregion

	#region Methods
	/// <summary>Adds the given <paramref name="symbol"/> to the scope.</summary>
	/// <param name="symbol">The symbol to add to the scope.</param>
	/// <exception cref="ArgumentException">
	/// 	Thrown if both the name and the declaration node of the given <paramref name="symbol"/> are <see langword="null"/>.
	/// </exception>
	public void Add(ISymbol symbol)
	{
		if (symbol.Name is null && symbol.Declaration is null)
			ThrowHelper.ThrowArgumentException(nameof(symbol), "The symbol's name and declaration node were both missing.");

		Count++;

		if (symbol.Name is not null)
		{
			if (_nameLookup.TryGetValue(symbol.Name, out SymbolGroup? symbols) is false)
			{
				symbols = [];
				_nameLookup.Add(symbol.Name, symbols);
			}

			symbols.Add(symbol);
		}

		if (symbol.Declaration is not null)
			_nodeLookup.Add(symbol.Declaration, symbol);
	}

	/// <inheritdoc/>
	public bool TryGet(string name, [NotNullWhen(true)] out ISymbolGroup? symbols)
	{
		if (_nameLookup.TryGetValue(name, out SymbolGroup? list))
		{
			Debug.Assert(list.Count > 0);
			symbols = list;

			return true;
		}

		if (Parent?.TryGet(name, out symbols) is true)
		{
			Debug.Assert(symbols.Count > 0);
			return true;
		}

		symbols = default;
		return false;
	}

	/// <inheritdoc/>
	public bool TryGet(IAbstractSyntaxNode declaration, [NotNullWhen(true)] out ISymbol? symbol)
	{
		if (_nodeLookup.TryGetValue(declaration, out symbol))
			return true;

		if (Parent?.TryGet(declaration, out symbol) is true)
			return true;

		symbol = default;
		return false;
	}

	/// <inheritdoc/>
	public override string ToString() => Name;
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"{nameof(SymbolScope)} {{ Name = ({Name}), Count = ({Count:n0} / {TotalCount:n0}) }}";
	#endregion
}
