namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols.Scopes;

public interface ISymbolScope
{
	#region Properties
	string Name { get; }
	ISymbolScope? Parent { get; }
	#endregion

	#region Methods
	bool TryGet(string name, [NotNullWhen(true)] out ISymbolGroup? symbols);
	ISymbolGroup GetAll(string name);
	void GetAll(string name, SymbolGroup destination);
	T Get<T>(ISyntaxNode declaration) where T : notnull, IDeclaredSymbol;
	ISymbolScope GetChild(ISyntaxNode declaration);
	void Update(IDeclaredSymbol symbol, ISyntaxNode declaration);
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
	public ISymbolGroup GetAll(string name)
	{
		SymbolGroup group = [];

		ISymbolScope? scope = this;
		while (scope is not null)
		{
			scope.GetAll(name, group);
			scope = scope.Parent;
		}

		return group;
	}
	public void GetAll(string name, SymbolGroup destination)
	{
		using (_nameLock.ReadLock())
		{
			if (_byName.TryGetValue(name, out SymbolGroup? group))
				destination.AddRange(group);
		}
	}
	public T Get<T>(ISyntaxNode declaration) where T : notnull, IDeclaredSymbol
	{
		using (_nodeLock.ReadLock())
			return (T)_byNode[declaration];
	}
	public void Update(IDeclaredSymbol symbol, ISyntaxNode declaration)
	{
		symbol.Declaration.ThrowIfInvalidShadow(declaration);

		using (_nodeLock.WriteLock())
		{
			_byNode.Remove(symbol.Declaration);
			symbol.Declaration = declaration;
			_byNode.Add(declaration, symbol);
		}
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

public static class ISymbolScopeExtensions
{
	extension(ISymbolScope scope)
	{
		#region Methods
		public void Get<T>(ISyntaxNode declaration, out T symbol) where T : notnull, IDeclaredSymbol
		{
			symbol = scope.Get<T>(declaration);
		}
		#endregion
	}
}