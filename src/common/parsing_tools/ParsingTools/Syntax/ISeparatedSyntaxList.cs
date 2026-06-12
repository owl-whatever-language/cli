namespace OwlDomain.ParsingTools.Syntax;

/// <summary>
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
public interface ISeparatedSyntaxList : ISyntaxList
{
	#region Properties
	/// <summary>The list of all of the syntax nodes in the list.</summary>
	IReadOnlyList<ISyntaxNode> Nodes { get; }

	/// <summary>The nodes that are acting as separators.</summary>
	IReadOnlyList<ISyntaxNode> Separators { get; }
	#endregion
}

/// <summary>
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
public interface ISeparatedSyntaxList<out TValue> : ISeparatedSyntaxList, ISyntaxList<TValue>
	where TValue : class, ISyntaxNode
{
}

/// <summary>
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
/// <typeparam name="TSeparator">The type of the separator nodes.</typeparam>
public interface ISeparatedSyntaxList<out TValue, out TSeparator> : ISeparatedSyntaxList<TValue>
	where TValue : class, ISyntaxNode
	where TSeparator : class, ISyntaxNode
{
	#region Properties
	/// <summary>The nodes that are acting as separators.</summary>
	new IReadOnlyList<TSeparator> Separators { get; }
	IReadOnlyList<ISyntaxNode> ISeparatedSyntaxList.Separators => Separators;
	#endregion
}
