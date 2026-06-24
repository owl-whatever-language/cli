namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols.Scopes;

public interface ISymbolScope
{
	#region Properties
	string Name { get; }
	ISymbolScope? Parent { get; }
	#endregion

	#region Methods
	bool TryGet(string name, [NotNullWhen(true)] out ISymbolGroup? symbols);
	T Get<T>(ISyntaxNode declaration) where T : notnull, IDeclaredSymbol;
	ISymbolScope GetChild(ISyntaxNode declaration);
	#endregion
}

public sealed class SymbolScope : ISymbolScope
{
	#region Fields
	private readonly Dictionary<string, SymbolGroup> _byName = [];
	private readonly Dictionary<ISyntaxNode, IDeclaredSymbol> _byNode = [];
	private readonly Dictionary<ISyntaxNode, ISymbolScope> _children = [];
	private readonly ReaderWriterLockSlim _nameLock = new();
	private readonly ReaderWriterLockSlim _nodeLock = new();
	private readonly ReaderWriterLockSlim _childrenLock = new();
	#endregion

	#region Properties
	public string Name { get; }
	public ISymbolScope? Parent { get; }
	#endregion

	#region Constructors
	public SymbolScope(string name) => Name = name;
	public SymbolScope(string name, ISymbolScope? parent) : this(name) => Parent = parent;
	#endregion

	#region Methods
	public void Add(ISymbol symbol)
	{
		string? name;

		if (symbol is IDeclaredSymbol declared)
		{
			name = declared.Name;
			using (_nodeLock.WriteLock())
				_byNode.Add(declared.Declaration, declared);
		}
		else
			name = symbol.Name;

		if (name is null)
			return;

		using (_nameLock.WriteLock())
		{
			if (_byName.TryGetValue(name, out SymbolGroup? group) is false)
			{
				group = [];
				_byName.Add(name, group);
			}

			group.Add(symbol);
		}
	}
	public bool TryGet(string name, [NotNullWhen(true)] out ISymbolGroup? symbols)
	{
		using (_nameLock.ReadLock())
		{
			if (_byName.TryGetValue(name, out SymbolGroup? group))
			{
				symbols = group;
				return true;
			}
		}

		if (Parent?.TryGet(name, out symbols) is true)
			return true;

		symbols = default;
		return false;
	}
	public T Get<T>(ISyntaxNode declaration) where T : notnull, IDeclaredSymbol
	{
		using (_nodeLock.ReadLock())
			return (T)_byNode[declaration];
	}

	public void Add(ISyntaxNode declaration, ISymbolScope scope)
	{
		using (_childrenLock.WriteLock())
			_children.Add(declaration, scope);
	}
	public ISymbolScope GetChild(ISyntaxNode declaration)
	{
		using (_childrenLock.ReadLock())
			return _children[declaration];
	}
	#endregion
}
