namespace OwlDomain.ParsingTools.Syntax;

public interface ISyntaxList<out TValue> : ISyntaxNode, IReadOnlyList<TValue>
	where TValue : class, ISyntaxNode
{
}

public interface ISyntaxList<out TValue, out TSeparator> : ISyntaxNode
	where TValue : class, ISyntaxNode
	where TSeparator : class, ISyntaxNode
{
	#region Properties
	IReadOnlyList<ISyntaxNode> Nodes { get; }
	IReadOnlyList<TValue> Values { get; }
	IReadOnlyList<TSeparator> Separators { get; }
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public class SyntaxList<TValue> : ISyntaxList<TValue>
	where TValue : class, ISyntaxNode
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly IReadOnlyList<TValue> _values;
	#endregion

	#region Properties
	public SyntaxNodeKind NodeKind => new(null, "syntax_list", null);
	public int Level => 0;

	/// <inheritdoc/>
	[DisallowNull]
	public virtual ISyntaxNode? Parent
	{
		get;
		set
		{
			if (field is not null)
				ThrowHelper.ThrowInvalidOperationException("The parent node has already been set.");

			field = value;
		}
	}

	/// <inheritdoc/>
	public IndexedPositionRange Position => GetChildren().GetPosition();

	/// <inheritdoc/>
	public IndexedPositionRange FullPosition => GetChildren().GetFullPosition();

	/// <inheritdoc/>
	public int Count => _values.Count;
	public bool IsFabricated => _values.Count is not 0 && _values.OrderByDescending(static c => c is ISyntaxToken).All(static c => c.IsFabricated);
	#endregion

	#region Indexers
	/// <inheritdoc/>
	public TValue this[int index] => _values[index];
	#endregion

	#region Constructors
	public SyntaxList() => _values = [];
	public SyntaxList(IReadOnlyList<TValue> values)
	{
		_values = values;

		AssignParentToChildren();
	}
	#endregion

	#region Methods
	private void AssignParentToChildren()
	{
		foreach (ISyntaxNode child in GetChildren())
			child.Parent = this;
	}

	/// <inheritdoc/>
	public IEnumerable<ISyntaxNode> GetChildren() => _values;
	TextFragmentCollection IDebugTreePrintable.GetFragments() => this.GetDebugFragments();

	/// <inheritdoc/>
	public IEnumerator<TValue> GetEnumerator() => _values.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"SyntaxList<{typeof(TValue).Name}> {{ Count = ({Count:n0}) }}";
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public class SyntaxList<TValue, TSeparator> : ISyntaxList<TValue, TSeparator>
	where TValue : class, ISyntaxNode
	where TSeparator : class, ISyntaxNode
{
	#region Properties
	public SyntaxNodeKind NodeKind => new(null, "syntax_list", null);
	public int Level => 0;

	/// <inheritdoc/>
	[DisallowNull]
	public ISyntaxNode? Parent
	{
		get;
		set
		{
			if (field is not null)
				ThrowHelper.ThrowInvalidOperationException("The parent node has already been set.");

			field = value;
		}
	}

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

	/// <inheritdoc/>
	public bool IsFabricated => Nodes.Count is not 0 && Nodes.OrderByDescending(static c => c is ISyntaxToken).All(static c => c.IsFabricated);
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

		AssignParentToChildren();
	}
	#endregion

	#region Methods
	private void AssignParentToChildren()
	{
		foreach (ISyntaxNode child in GetChildren())
			child.Parent = this;
	}

	/// <inheritdoc/>
	public IEnumerable<ISyntaxNode> GetChildren() => Nodes;
	TextFragmentCollection IDebugTreePrintable.GetFragments() => this.GetDebugFragments();
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"SyntaxList<{typeof(TValue).Name},{typeof(TSeparator).Name}> {{ Values = ({Values.Count:n0}), Separators = ({Separators.Count:n0}) }}";
	#endregion
}
