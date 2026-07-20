namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols.Scopes;

public interface ISymbolScope : IDebugTextFactory
{
	#region Properties
	string Name { get; }
	ISymbolScope? Parent { get; }
	ICoreSymbolScope Core { get; }
	#endregion

	#region Methods
	bool TryGetLocal(string name, [NotNullWhen(true)] out ISymbolGroup? symbols);
	bool TryGet(string name, [NotNullWhen(true)] out ISymbolGroup? symbols);
	ISymbolGroup GetAll(string name);
	void GetAll(string name, SymbolGroup destination);
	T Get<T>(ISyntaxNode declaration) where T : notnull, IDeclaredSymbol;
	ISymbolGroup GetAllNamed(bool includeParent = true);
	void GetAllNamed(SymbolGroup destination);
	ISymbolScope GetChild(ISyntaxNode declaration);
	void Update(IDeclaredSymbol symbol, ISyntaxNode declaration);
	void UpdateChild(ISyntaxNode oldDeclaration, ISyntaxNode newDeclaration);
	#endregion
}

public class SymbolScope : ISymbolScope
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
	public virtual ICoreSymbolScope Core => Parent?.Core ?? ThrowHelper.ThrowInvalidOperationException<ICoreSymbolScope>("The root scope wasn't the core one.");
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
	public bool TryGetLocal(string name, [NotNullWhen(true)] out ISymbolGroup? symbols)
	{
		using (_nameLock.ReadLock())
		{
			if (_byName.TryGetValue(name, out SymbolGroup? group))
			{
				symbols = group;
				return true;
			}
		}
		symbols = default;
		return false;
	}
	public bool TryGet(string name, [NotNullWhen(true)] out ISymbolGroup? symbols)
	{
		if (TryGetLocal(name, out symbols))
			return true;

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
	public ISymbolGroup GetAllNamed(bool includeParent = true)
	{
		SymbolGroup group = [];

		ISymbolScope? scope = this;
		while (scope is not null)
		{
			scope.GetAllNamed(group);
			scope = scope.Parent;

			if (includeParent is false)
				break;
		}

		return group;
	}
	public void GetAllNamed(SymbolGroup destination)
	{
		using (_nameLock.ReadLock())
		{
			foreach (SymbolGroup group in _byName.Values)
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
	public void UpdateChild(ISyntaxNode oldDeclaration, ISyntaxNode newDeclaration)
	{
		oldDeclaration.ThrowIfInvalidShadow(newDeclaration);
		using (_childrenLock.WriteLock())
		{
			_children.Remove(oldDeclaration, out ISymbolScope? actualScope);
			if (actualScope is null)
				ThrowHelper.ThrowArgumentException(nameof(oldDeclaration), "There was no existing child scope that was associated with the given declaration.");

			_children.Add(newDeclaration, actualScope);
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

	public TextFragmentCollection GetDebugText() => [new(Name)];
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
		public ISymbolGroup GetAlternative(string name) => scope.GetAllNamed().GetAlternative(name);
		public ISymbolGroup GetAlternative<T>(string name) where T : notnull, ISymbol
		{
			return scope
			.GetAllNamed()
			.OfType<T>()
			.ToGroup()
			.GetAlternative(name);
		}
		#endregion
	}
}
