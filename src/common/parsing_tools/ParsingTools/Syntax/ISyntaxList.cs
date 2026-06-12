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
