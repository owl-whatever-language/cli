namespace OwlDomain.ParsingTools.Syntax;

/// <summary>
/// 	Represents a general syntax list.
/// </summary>
public interface ISyntaxList : ISyntaxNode
{
	#region Properties
	/// <summary>The values stored in the syntax list.</summary>
	IReadOnlyList<ISyntaxNode> Values { get; }
	#endregion
}

/// <summary>
/// 	Represents a general syntax list.
/// </summary>
/// <typeparam name="TValue">The type of the syntax nodes that the list stores.</typeparam>
public interface ISyntaxList<out TValue> : ISyntaxList
	where TValue : class, ISyntaxNode
{
	#region Properties
	/// <summary>The values stored in the syntax list.</summary>
	new IReadOnlyList<TValue> Values { get; }
	IReadOnlyList<ISyntaxNode> ISyntaxList.Values => Values;
	#endregion
}

/// <summary>
/// 	Represents a general syntax list.
/// </summary>
/// <typeparam name="TValue">The type of the syntax nodes that the list stores.</typeparam>
public class SyntaxList<TValue> : ISyntaxList<TValue>
	where TValue : class, ISyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public SyntaxKind Kind => SyntaxKind.SyntaxList;

	/// <inheritdoc/>
	public IndexedPositionRange Position
	{
		get
		{
			ISyntaxNode? first = Values.FirstOrDefault();
			if (first is null)
				return default;

			ISyntaxNode last = Values.Last();

			return new(first.Position.Start, last.Position.End);
		}
	}

	/// <inheritdoc/>
	public IReadOnlyList<TValue> Values { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="SyntaxList{T}"/> instance.</summary>
	/// <param name="values">The value nodes stored in the list.</param>
	public SyntaxList(IReadOnlyList<TValue> values)
	{
		Values = values;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public IEnumerable<ISyntaxNode> GetChildren() => Values;
	#endregion
}
