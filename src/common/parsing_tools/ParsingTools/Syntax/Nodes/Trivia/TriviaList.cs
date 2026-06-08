namespace OwlDomain.ParsingTools.Syntax.Nodes.Trivia;

/// <summary>
/// 	Represents a list of trivia nodes in the concrete syntax tree (CST).
/// </summary>
public sealed class TriviaList : IConcreteSyntaxNode, IReadOnlyList<ITriviaNode>
{
	#region Fields
	private readonly IReadOnlyList<ITriviaNode> _nodes;
	#endregion

	#region Properties
	/// <inheritdoc/>
	public SyntaxKind Kind => SyntaxKind.TriviaList;

	/// <inheritdoc/>
	public IndexedPositionRange Position
	{
		get
		{
			if (_nodes.Count is 0)
				return default;

			IndexedLinePosition start = _nodes[0].Position.Start;
			IndexedLinePosition end = _nodes[^1].Position.End;

			return new(start, end);
		}
	}

	/// <inheritdoc/>
	public IndexedPositionRange FullPosition => Position;

	/// <inheritdoc/>
	public int Count => _nodes.Count;

	/// <summary>Whether the trivia list is empty.</summary>
	public bool IsEmpty => Count is 0;

	/// <inheritdoc/>
	public bool IsFabricated => GetChildren().All(n => n.IsFabricated);
	#endregion

	#region Indexers
	/// <inheritdoc/>
	public ITriviaNode this[int index] => _nodes[index];
	#endregion

	#region Constructors
	/// <summary>Creates a new, empty trivia list.</summary>
	public TriviaList() { _nodes = []; }

	/// <summary>Creates a new trivia list.</summary>
	/// <param name="nodes">The nodes that are a part of the trivia list.</param>
	/// <exception cref="ArgumentException">Thrown if the given list of trivia <paramref name="nodes"/> is not ordered.</exception>
	public TriviaList(IReadOnlyList<ITriviaNode> nodes)
	{
		Guard.IsOrdered(nodes);

		_nodes = nodes;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public IEnumerable<IConcreteSyntaxNode> GetChildren() => _nodes;

	/// <inheritdoc/>
	public IEnumerator<ITriviaNode> GetEnumerator() => _nodes.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	#endregion
}
