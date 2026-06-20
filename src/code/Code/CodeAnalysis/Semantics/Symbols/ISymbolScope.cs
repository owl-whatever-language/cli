namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols;

public interface ISymbolScope : IReadOnlyCollection<ISymbol>
{
	#region Properties
	string Name { get; }
	ISymbolScope? Parent { get; }
	IReadOnlyCollection<ISymbolScope> Children { get; }
	#endregion

	#region Methods
	bool TryGetSymbol(string name, [NotNullWhen(true)] out ISymbolGroup? symbols);
	bool TryGetSymbol(IConcreteSyntaxNode node, [NotNullWhen(true)] out ISymbol? symbol);
	T GetTarget<T>(IConcreteSyntaxNode node) where T : notnull, ISymbolTarget;
	bool TryGetChild(IConcreteSyntaxNode node, [NotNullWhen(true)] out ISymbolScope? scope);
	ISymbolScope GetChild(IConcreteSyntaxNode node);
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class SymbolScope : ISymbolScope
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<SymbolScope> _children = [];

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly Dictionary<IConcreteSyntaxNode, ISymbolScope> _childrenByNode = [];

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<ISymbol> _symbols = [];

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly Dictionary<string, SymbolGroup> _byName = [];

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly Dictionary<IConcreteSyntaxNode, ISymbol> _byNode = [];
	#endregion

	#region Properties
	public string Name { get; }
	public ISymbolScope? Parent { get; }

	/// <inheritdoc/>
	public IReadOnlyCollection<SymbolScope> Children => _children;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyCollection<ISymbolScope> ISymbolScope.Children => _children;

	public int Count => _symbols.Count;
	#endregion

	#region Constructors
	public SymbolScope(string name) => Name = name;
	public SymbolScope(string name, ISymbolScope parent) : this(name) => Parent = parent;
	#endregion

	#region Methods
	public SymbolScope NestScope(string name, IConcreteSyntaxNode? declaration)
	{
		SymbolScope child = new(name, this);
		_children.Add(child);

		if (declaration is not null)
			_childrenByNode.Add(declaration, child);

		return child;
	}
	public bool TryGetChild(IConcreteSyntaxNode node, [NotNullWhen(true)] out ISymbolScope? scope)
	{
		return _childrenByNode.TryGetValue(node, out scope);
	}
	public ISymbolScope GetChild(IConcreteSyntaxNode node)
	{
		if (TryGetChild(node, out ISymbolScope? scope))
			return scope;

		ThrowHelper.ThrowInvalidOperationException("Couldn't find a child scope for the given declaration node but one was expected.");
		return default;
	}

	public void AddSymbol(ISymbolTarget target) => AddSymbol(target.Symbol);
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

		_symbols.Add(symbol);
	}
	public bool TryGetSymbol(string name, [NotNullWhen(true)] out ISymbolGroup? symbols)
	{
		if (_byName.TryGetValue(name, out SymbolGroup? group))
		{
			symbols = group;
			return true;
		}

		if (Parent is not null)
			return Parent.TryGetSymbol(name, out symbols);

		symbols = default;
		return false;
	}

	public bool TryGetSymbol(IConcreteSyntaxNode node, [NotNullWhen(true)] out ISymbol? symbol)
	{
		if (_byNode.TryGetValue(node, out symbol))
			return true;

		if (Parent is not null)
			return Parent.TryGetSymbol(node, out symbol);

		symbol = default;
		return false;
	}

	public T GetTarget<T>(IConcreteSyntaxNode node) where T : notnull, ISymbolTarget
	{
		if (TryGetSymbol(node, out ISymbol? symbol) is false)
			ThrowHelper.ThrowInvalidOperationException($"Couldn't find a symbol for the given node, but one should've existed.");

		if (symbol.Target is T typed)
			return typed;

		ThrowHelper.ThrowInvalidOperationException($"The found symbol ({symbol}) did not point to the expected target type ({typeof(T).Name}).");
		return default;
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
