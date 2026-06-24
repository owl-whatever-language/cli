namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Symbols.Scopes;

public interface ISymbolScope
{
	#region Properties
	string Name { get; }
	ISymbolScope? Parent { get; }
	#endregion

	#region Methods
	bool TryGet(string name, [NotNullWhen(true)] out ISymbolGroup? symbols);
	#endregion
}

public sealed class SymbolScope : ISymbolScope
{
	#region Fields
	private readonly Dictionary<string, SymbolGroup> _symbols = [];
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
			name = declared.Name;
		else
			name = symbol.Name;

		if (name is null)
			return;

		if (_symbols.TryGetValue(name, out SymbolGroup? group) is false)
		{
			group = [];
			_symbols.Add(name, group);
		}

		group.Add(symbol);
	}
	public bool TryGet(string name, [NotNullWhen(true)] out ISymbolGroup? symbols)
	{
		if (_symbols.TryGetValue(name, out SymbolGroup? group))
		{
			symbols = group;
			return true;
		}

		if (Parent?.TryGet(name, out symbols) is true)
			return true;

		symbols = default;
		return false;
	}
	#endregion
}
