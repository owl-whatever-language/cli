namespace OwlDomain.ParsingTools.Semantics.Symbols;

public interface ISymbolScope : IDebugTextFactory
{
	#region Properties
	ISymbolScope? Parent { get; }
	string Name { get; }
	#endregion

	#region Methods
	bool TryGet(ISyntaxNode declaration, [NotNullWhen(true)] out IMutableDeclaredSymbol? symbol);
	bool TryGet(ISyntaxNode declaration, [NotNullWhen(true)] out IMutableDeclaredSymbolScope? scope);
	SymbolSearchResult Search(string name, bool includeParents = true);
	SymbolSearchResult GetNamed(bool includeParents = true);
	#endregion
}

public interface IMutableSymbolScope : ISymbolScope
{
	#region Methods
	void Add(ISymbol symbol);
	IMutableDeclaredSymbolScope AddScope(IDeclaredSymbol symbol);
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public class SymbolScope : IMutableSymbolScope
{
	#region Fields
	private readonly ReaderWriterLockSlim _lock = new();
	private readonly Dictionary<ISyntaxNode, IMutableDeclaredSymbolScope> _scopes = [];
	private readonly Dictionary<string, IMutableSymbolCollection> _byName = [];
	private readonly Dictionary<ISyntaxNode, IMutableDeclaredSymbol> _byNode = [];

	private int _symbolCount;
	private int _scopeCount;
	#endregion

	#region Properties
	public ISymbolScope? Parent { get; }
	public string Name { get; }
	#endregion

	#region Constructors
	public SymbolScope(string name) => Name = name;
	public SymbolScope(ISymbolScope? parent, string name) : this(name) => Parent = parent;
	#endregion

	#region Methods
	public bool TryGet(ISyntaxNode declaration, [NotNullWhen(true)] out IMutableDeclaredSymbol? symbol)
	{
		using (_lock.ReadLock())
		{
			if (_byNode.TryGetValue(declaration, out symbol))
				return true;
		}

		if (Parent?.TryGet(declaration, out symbol) is true)
			return true;

		return false;
	}
	public bool TryGet(ISyntaxNode declaration, [NotNullWhen(true)] out IMutableDeclaredSymbolScope? scope)
	{
		using (_lock.ReadLock())
		{
			if (_scopes.TryGetValue(declaration, out scope))
				return true;
		}

		if (Parent?.TryGet(declaration, out scope) is true)
			return true;

		return false;
	}

	public SymbolSearchResult Search(string name, bool includeParents = true)
	{
		SymbolCollection all = [];
		ISymbolCollection? fromFirst = null;
		SymbolCollection fromCurrent = [];
		SymbolCollection fromParents = [];

		using (_lock.ReadLock())
		{
			if (_byName.TryGetValue(name, out IMutableSymbolCollection? named))
			{
				all.AddRange(named);
				fromCurrent.AddRange(named);

				if (named.Any())
					fromFirst ??= named;
			}
		}

		if (includeParents is false)
			return new(all, fromFirst ?? SymbolCollection.Empty, fromCurrent, fromParents);

		ISymbolScope? parent = Parent;
		while (parent is not null)
		{
			SymbolSearchResult result = parent.Search(name, false);

			all.AddRange(result.All);
			fromParents.AddRange(result.FromCurrent);

			if (result.FromCurrent.Any())
				fromFirst ??= result.FromCurrent;

			parent = parent.Parent;
		}

		return new(all, fromFirst ?? SymbolCollection.Empty, fromCurrent, fromParents);
	}
	public SymbolSearchResult GetNamed(bool includeParents = true)
	{
		SymbolCollection all = [];
		ISymbolCollection? fromFirst = null;
		SymbolCollection fromCurrent = [];
		SymbolCollection fromParents = [];

		using (_lock.ReadLock())
		{
			foreach (ISymbolCollection current in _byName.Values)
			{
				all.AddRange(current);
				fromCurrent.AddRange(current);

				if (current.Any())
					fromFirst ??= current;
			}
		}

		if (includeParents is false)
			return new(all, fromFirst ?? SymbolCollection.Empty, fromCurrent, fromParents);

		ISymbolScope? parent = Parent;
		while (parent is not null)
		{
			SymbolSearchResult result = parent.GetNamed(false);

			all.AddRange(result.All);
			fromParents.AddRange(result.FromCurrent);

			if (result.FromCurrent.Any())
				fromFirst ??= result.FromCurrent;

			parent = parent.Parent;
		}

		return new(all, fromFirst ?? SymbolCollection.Empty, fromCurrent, fromParents);
	}
	#endregion

	#region Mutable methods
	public void Add(ISymbol symbol)
	{
		Interlocked.Increment(ref _symbolCount);

		if (symbol is IDeclaredSymbol declared)
		{
			if (declared is not IMutableDeclaredSymbol mutable)
			{
				ThrowHelper.ThrowArgumentException(nameof(symbol), "Expected the declared symbol to be mutable.");
				return;
			}

			using (_lock.WriteLock())
			{
				_byNode.Add(declared.Declaration, mutable);
				mutable.Shadowed += DeclaredSymbolShadowed;
			}

			if (declared.Name is null)
				return;
		}
		else if (symbol.Name is null)
			ThrowHelper.ThrowArgumentException(nameof(symbol), "Expected the non-declared symbol to have a name.");

		using (_lock.WriteLock())
		{
			Debug.Assert(symbol.Name is not null);
			if (_byName.TryGetValue(symbol.Name, out IMutableSymbolCollection? collection) is false)
			{
				collection = new SymbolCollection();
				_byName.Add(symbol.Name, collection);
			}

			collection.Add(symbol);
		}
	}
	public IMutableDeclaredSymbolScope AddScope(IDeclaredSymbol symbol)
	{
		Interlocked.Increment(ref _scopeCount);

		using (_lock.WriteLock())
		{
			DeclaredSymbolScope scope = new(this, symbol.Declaration, $"{symbol.Declaration.NodeKind.Name}({symbol.Name})");
			scope.Shadowed += SymbolScopeShadowed;

			_scopes.Add(symbol.Declaration, scope);

			return scope;
		}
	}
	#endregion

	#region Shadowing callbacks
	private void DeclaredSymbolShadowed(IMutableDeclaredSymbol symbol, ISyntaxNode oldDeclaration, ISyntaxNode newDeclaration)
	{
		using (_lock.WriteLock())
		{
			_byNode.Remove(oldDeclaration);
			_byNode.Add(newDeclaration, symbol);
		}
	}
	private void SymbolScopeShadowed(IMutableDeclaredSymbolScope scope, ISyntaxNode oldDeclaration, ISyntaxNode newDeclaration)
	{
		using (_lock.WriteLock())
		{
			_scopes.Remove(oldDeclaration);
			_scopes.Add(newDeclaration, scope);
		}
	}
	#endregion

	#region Helpers
	public TextFragmentCollection GetDebugText() => [new(Name)];
	private string DebuggerDisplay() => $"Scope: {Name} {{ Symbols = ({_symbolCount}, Scopes = ({_scopeCount}) }}";
	#endregion
}

public static class SymbolScopeExtensions
{
	extension(ISymbolScope scope)
	{
		#region Properties
		public ISymbolScope Root
		{
			get
			{
				ISymbolScope current = scope;
				while (current.Parent is not null)
					current = current.Parent;

				return current;
			}
		}
		#endregion

		#region Methods
		public IMutableDeclaredSymbol Get(ISyntaxNode node)
		{
			if (scope.TryGet(node, out IMutableDeclaredSymbol? symbol) is false)
				ThrowHelper.ThrowArgumentException(nameof(node), "The given syntax node was not linked to a declared symbol in this scope chain.");

			return symbol;
		}
		public T Get<T>(ISyntaxNode node) where T : notnull, IMutableDeclaredSymbol
		{
			return (T)Get(scope, node);
		}
		public IMutableDeclaredSymbolScope GetScope(ISyntaxNode node)
		{
			if (scope.TryGet(node, out IMutableDeclaredSymbolScope? declaredScope) is false)
				ThrowHelper.ThrowArgumentException(nameof(node), "The given syntax node was not linked to a declared symbol scope in this scope chain.");

			return declaredScope;
		}

		public bool TrySearch(string name, out SymbolSearchResult result) => TrySearch(scope, name, true, out result);
		public bool TrySearch(string name, bool includeParents, out SymbolSearchResult result)
		{
			result = scope.Search(name, includeParents);
			return result.All.Any();
		}

		public bool TrySearch(string name, [NotNullWhen(true)] out ISymbolCollection? symbols) => TrySearch(scope, name, true, out symbols);
		public bool TrySearch(string name, bool includeParents, [NotNullWhen(true)] out ISymbolCollection? symbols)
		{
			SymbolSearchResult result = scope.Search(name, includeParents);
			if (result.All.Any())
			{
				symbols = result.All;
				return true;
			}

			symbols = default;
			return false;
		}

		public bool TrySearchFirst(string name, [NotNullWhen(true)] out ISymbolCollection? symbols) => TrySearchFirst(scope, name, true, out symbols);
		public bool TrySearchFirst(string name, bool includeParents, [NotNullWhen(true)] out ISymbolCollection? symbols)
		{
			SymbolSearchResult result = scope.Search(name, includeParents);
			if (result.FromFirst.Any())
			{
				symbols = result.FromFirst;
				return true;
			}

			symbols = default;
			return false;
		}

		public ISymbolCollection GetAlternative(string? name) => scope.GetNamed().All.GetAlternative(name);
		public ISymbolCollection GetAlternative<T>(string? name) where T : notnull, ISymbol
		{
			return scope
			.GetNamed().All
			.OfType<T>()
			.ToCollection()
			.GetAlternative(name);
		}
		#endregion
	}
}
