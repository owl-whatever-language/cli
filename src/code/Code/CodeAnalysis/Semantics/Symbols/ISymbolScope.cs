namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols;

public interface ISymbolScope : IReadOnlyCollection<ISymbol>
{
	#region Properties
	string Name { get; }
	ISymbolScope? Parent { get; }
	IReadOnlyCollection<ISymbolScope> Children { get; }
	#endregion

	#region Methods
	bool GetSymbol(string name, [NotNullWhen(true)] out ISymbolGroup? symbols);
	bool GetSymbol(IConcreteSyntaxNode node, [NotNullWhen(true)] out ISymbol? symbol);
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class SymbolScope : ISymbolScope
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<SymbolScope> _children = [];

	private readonly List<ISymbol> _symbols = [];
	private readonly Dictionary<string, SymbolGroup> _byName = [];
	private readonly Dictionary<IConcreteSyntaxNode, ISymbol> _byNode = [];
	#endregion

	#region Properties
	public string Name { get; }
	public ISymbolScope? Parent { get; }

	/// <inheritdoc/>
	public IReadOnlyCollection<SymbolScope> Children => _children;
	IReadOnlyCollection<ISymbolScope> ISymbolScope.Children => _children;

	public int Count => _symbols.Count;
	#endregion

	#region Constructors
	public SymbolScope(string name) => Name = name;
	public SymbolScope(string name, ISymbolScope parent) : this(name) => Parent = parent;
	#endregion

	#region Methods
	public SymbolScope NestScope(string name)
	{
		SymbolScope child = new(name, this);
		_children.Add(child);

		return child;
	}

	public void AddSymbol(ISymbol symbol)
	{
		if (symbol is IDeclaredSymbol declared)
			_byNode.Add(declared.Declaration, symbol);
		else if (symbol.Name is null)
			ThrowHelper.ThrowArgumentException(nameof(symbol), "Symbols that aren't declared cannot have a null name.");

		if (symbol.Name is not null)
		{
			if (_byName.TryGetValue(symbol.Name, out SymbolGroup? group) is false)
			{
				group = [];
				_byName.Add(symbol.Name, group);
			}

			group.Add(symbol);
		}
	}
	public bool GetSymbol(string name, [NotNullWhen(true)] out ISymbolGroup? symbols)
	{
		if (_byName.TryGetValue(name, out SymbolGroup? group))
		{
			symbols = group;
			return true;
		}

		if (Parent is not null)
			return Parent.GetSymbol(name, out symbols);

		symbols = default;
		return false;
	}

	public bool GetSymbol(IConcreteSyntaxNode node, [NotNullWhen(true)] out ISymbol? symbol)
	{
		if (_byNode.TryGetValue(node, out symbol))
			return true;

		if (Parent is not null)
			return Parent.GetSymbol(node, out symbol);

		symbol = default;
		return false;
	}

	public IEnumerator<ISymbol> GetEnumerator() => _symbols.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	#endregion

	#region Helpers
	private string DebuggerDisplay()
	{
		if (Parent is not null)
			return $"{nameof(SymbolScope)}({Name}) {{ Count = ({Count:n0}), Parent = ({Parent.Name}) }}";

		return $"{nameof(SymbolScope)}({Name}) {{ Count = ({Count:n0}) }}";
	}
	#endregion
}
