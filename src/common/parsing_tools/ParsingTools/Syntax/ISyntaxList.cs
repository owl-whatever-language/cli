namespace OwlDomain.ParsingTools.Syntax;

public interface ISyntaxList<TValue> : ISyntaxNode, IReadOnlyList<TValue>
	where TValue : notnull, ISyntaxNode
{
}

public interface ISyntaxList<TValue, TSeparator> : ISyntaxNode
	where TValue : notnull, ISyntaxNode
	where TSeparator : notnull, ISyntaxNode
{
	#region Properties
	IReadOnlyList<ISyntaxNode> Nodes { get; }
	IReadOnlyList<TValue> Values { get; }
	IReadOnlyList<TSeparator> Separators { get; }
	#endregion
}

public class SyntaxList<TValue> : ISyntaxList<TValue>
	where TValue : class, ISyntaxNode
{
	#region Fields
	private readonly IReadOnlyList<TValue> _values;
	#endregion

	#region Properties
	/// <inheritdoc/>
	public IndexedPositionRange Position => GetChildren().GetPosition();

	/// <inheritdoc/>
	public IndexedPositionRange FullPosition => GetChildren().GetFullPosition();

	/// <inheritdoc/>
	public int Count => _values.Count;
	#endregion

	#region Indexers
	/// <inheritdoc/>
	public TValue this[int index] => _values[index];
	#endregion

	#region Constructors
	public SyntaxList() => _values = [];
	public SyntaxList(IReadOnlyList<TValue> values) => _values = values;
	#endregion

	#region Methods
	/// <inheritdoc/>
	public IEnumerable<ISyntaxNode> GetChildren() => _values;

	/// <inheritdoc/>
	public IEnumerator<TValue> GetEnumerator() => _values.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	#endregion
}

public class SyntaxList<TValue, TSeparator> : ISyntaxList<TValue, TSeparator>
	where TValue : notnull, ISyntaxNode
	where TSeparator : notnull, ISyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public IReadOnlyList<ISyntaxNode> Nodes { get; }

	/// <inheritdoc/>
	public IReadOnlyList<TValue> Values { get; }

	/// <inheritdoc/>
	public IReadOnlyList<TSeparator> Separators { get; }

	/// <inheritdoc/>
	public IndexedPositionRange Position => Nodes.GetPosition();

	/// <inheritdoc/>
	public IndexedPositionRange FullPosition => Nodes.GetFullPosition();
	#endregion

	#region Constructors
	public SyntaxList()
	{
		Nodes = [];
		Values = [];
		Separators = [];
	}
	public SyntaxList(IReadOnlyList<ISyntaxNode> nodes, IReadOnlyList<TValue> values, IReadOnlyList<TSeparator> separators)
	{
		Nodes = nodes;
		Values = values;
		Separators = separators;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public IEnumerable<ISyntaxNode> GetChildren() => Nodes;
	#endregion
}
