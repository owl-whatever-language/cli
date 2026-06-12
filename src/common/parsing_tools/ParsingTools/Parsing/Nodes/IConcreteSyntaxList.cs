namespace OwlDomain.ParsingTools.Parsing.Nodes;

/// <summary>
/// 	Represents a concrete syntax list.
/// </summary>
public interface IConcreteSyntaxList : ISyntaxList, IConcreteSyntaxNode
{
}

/// <summary>
/// 	Represents a concrete syntax list.
/// </summary>
/// <typeparam name="TValue">The type of the concrete syntax nodes that the list stores.</typeparam>
public interface IConcreteSyntaxList<out TValue> : ISyntaxList<TValue>, IConcreteSyntaxList
	where TValue : class, IConcreteSyntaxNode
{
}

/// <summary>
/// 	Represents a concrete syntax list.
/// </summary>
/// <typeparam name="TValue">The type of the concrete syntax nodes that the list stores.</typeparam>
public class ConcreteSyntaxList<TValue> : BaseConcreteSyntaxNode, IConcreteSyntaxList<TValue>, IReadOnlyList<TValue>
	where TValue : class, IConcreteSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public override SyntaxKind Kind => SyntaxKind.SyntaxList;

	/// <inheritdoc/>
	public IReadOnlyList<TValue> Values { get; }

	/// <inheritdoc/>
	public int Count => Values.Count;
	#endregion

	#region Indexers
	/// <inheritdoc/>
	public TValue this[int index] => Values[index];
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="ConcreteSyntaxList{TValue}"/> instance.</summary>
	public ConcreteSyntaxList() => Values = [];

	/// <summary>Creates a new <see cref="ConcreteSyntaxList{TValue}"/> instance.</summary>
	/// <param name="values">The values to store in the list.</param>
	public ConcreteSyntaxList(IReadOnlyList<TValue> values)
	{
		Guard.IsOrdered(values);

		Values = values;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override IEnumerable<TValue> GetChildren() => Values;

	/// <inheritdoc/>
	public IEnumerator<TValue> GetEnumerator() => Values.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	#endregion
}
