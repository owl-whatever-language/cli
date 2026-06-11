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

/// <summary>
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
/// <typeparam name="TSeparator">The type of the separator nodes.</typeparam>
public class SeparatedSyntaxList<TValue, TSeparator> : ISeparatedSyntaxList<TValue, TSeparator>
	where TValue : class, ISyntaxNode
	where TSeparator : class, ISyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public SyntaxKind Kind => SyntaxKind.SeparatedSyntaxList;

	/// <inheritdoc/>
	public IndexedPositionRange Position
	{
		get
		{
			ISyntaxNode? first = Nodes.FirstOrDefault();
			if (first is null)
				return default;

			ISyntaxNode last = Nodes.Last();

			return new(first.Position.Start, last.Position.End);
		}
	}

	/// <inheritdoc/>
	public IReadOnlyList<ISyntaxNode> Nodes { get; }

	/// <inheritdoc/>
	public IReadOnlyList<TValue> Values { get; }

	/// <inheritdoc/>
	public IReadOnlyList<TSeparator> Separators { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="SeparatedSyntaxList{TValue, TSeparator}"/> instance.</summary>
	/// <param name="nodes">All of the nodes in the list.</param>
	/// <param name="values">The value nodes in the list.</param>
	/// <param name="separators">The separator nodes in the list.</param>
	public SeparatedSyntaxList(IReadOnlyList<ISyntaxNode> nodes, IReadOnlyList<TValue> values, IReadOnlyList<TSeparator> separators)
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

/// <summary>
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
public sealed class SeparatedSyntaxList<TValue> : SeparatedSyntaxList<TValue, ITokenNode>
	where TValue : class, ISyntaxNode
{
	#region Constructors
	/// <summary>Creates a new <see cref="SeparatedSyntaxList{TValue}"/> instance.</summary>
	/// <param name="nodes">All of the nodes in the list.</param>
	/// <param name="values">The value nodes in the list.</param>
	/// <param name="separators">The separator nodes in the list.</param>
	public SeparatedSyntaxList(IReadOnlyList<ISyntaxNode> nodes, IReadOnlyList<TValue> values, IReadOnlyList<ITokenNode> separators) : base(nodes, values, separators)
	{
	}
	#endregion
}
