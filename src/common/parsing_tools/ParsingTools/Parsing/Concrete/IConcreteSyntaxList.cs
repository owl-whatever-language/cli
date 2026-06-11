namespace OwlDomain.ParsingTools.Parsing.Concrete;

/// <summary>
/// 	Represents a general syntax list.
/// </summary>
public interface IConcreteSyntaxList : ISyntaxList, IConcreteSyntaxNode
{
	#region Properties
	/// <summary>The values stored in the syntax list.</summary>
	new IReadOnlyList<IConcreteSyntaxNode> Values { get; }
	IReadOnlyList<ISyntaxNode> ISyntaxList.Values => Values;
	#endregion
}

/// <summary>
/// 	Represents a general syntax list.
/// </summary>
/// <typeparam name="TValue">The type of the syntax nodes that the list stores.</typeparam>
public interface IConcreteSyntaxList<out TValue> : ISyntaxList<TValue>, IConcreteSyntaxList
	where TValue : class, IConcreteSyntaxNode
{
	#region Properties
	/// <summary>The values stored in the syntax list.</summary>
	new IReadOnlyList<TValue> Values { get; }
	IReadOnlyList<ISyntaxNode> ISyntaxList.Values => Values;
	IReadOnlyList<IConcreteSyntaxNode> IConcreteSyntaxList.Values => Values;
	#endregion
}

/// <summary>
/// 	Represents a general syntax list.
/// </summary>
/// <typeparam name="TValue">The type of the syntax nodes that the list stores.</typeparam>
public class ConcreteSyntaxList<TValue> : BaseConcreteSyntaxNode, IConcreteSyntaxList<TValue>
	where TValue : class, IConcreteSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public override SyntaxKind Kind => SyntaxKind.SyntaxList;

	/// <inheritdoc/>
	public IReadOnlyList<TValue> Values { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="ConcreteSyntaxList{TValue}"/> instance.</summary>
	/// <param name="values">The value nodes stored in the list.</param>
	public ConcreteSyntaxList(IReadOnlyList<TValue> values)
	{
		Values = values;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override IEnumerable<IConcreteSyntaxNode> GetChildren() => Values;
	#endregion
}
