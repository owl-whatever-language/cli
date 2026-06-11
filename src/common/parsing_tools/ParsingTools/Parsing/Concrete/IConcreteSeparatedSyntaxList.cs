namespace OwlDomain.ParsingTools.Parsing.Concrete;

/// <summary>
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
public interface IConcreteSeparatedSyntaxList : ISeparatedSyntaxList, IConcreteSyntaxList, IConcreteSyntaxNode
{
	#region Properties
	/// <summary>The list of all of the syntax nodes in the list.</summary>
	new IReadOnlyList<IConcreteSyntaxNode> Nodes { get; }
	IReadOnlyList<ISyntaxNode> ISeparatedSyntaxList.Nodes => Nodes;

	/// <summary>The nodes that are acting as separators.</summary>
	new IReadOnlyList<IConcreteSyntaxNode> Separators { get; }
	IReadOnlyList<ISyntaxNode> ISeparatedSyntaxList.Separators => Separators;
	#endregion
}

/// <summary>
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
public interface IConcreteSeparatedSyntaxList<out TValue> : IConcreteSeparatedSyntaxList, ISeparatedSyntaxList<TValue>, IConcreteSyntaxList<TValue>
	where TValue : class, IConcreteSyntaxNode
{
}

/// <summary>
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
/// <typeparam name="TSeparator">The type of the separator nodes.</typeparam>
public interface IConcreteSeparatedSyntaxList<out TValue, out TSeparator> : IConcreteSeparatedSyntaxList<TValue>, ISeparatedSyntaxList<TValue, TSeparator>
	where TValue : class, IConcreteSyntaxNode
	where TSeparator : class, IConcreteSyntaxNode
{
	#region Properties
	/// <summary>The nodes that are acting as separators.</summary>
	new IReadOnlyList<TSeparator> Separators { get; }
	IReadOnlyList<ISyntaxNode> ISeparatedSyntaxList.Separators => Separators;
	IReadOnlyList<IConcreteSyntaxNode> IConcreteSeparatedSyntaxList.Separators => Separators;
	#endregion
}

/// <summary>
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
/// <typeparam name="TSeparator">The type of the separator nodes.</typeparam>
public class ConcreteSeparatedSyntaxList<TValue, TSeparator> : BaseConcreteSyntaxNode, IConcreteSeparatedSyntaxList<TValue, TSeparator>
	where TValue : class, IConcreteSyntaxNode
	where TSeparator : class, IConcreteSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public override SyntaxKind Kind => SyntaxKind.SeparatedSyntaxList;

	/// <inheritdoc/>
	public IReadOnlyList<IConcreteSyntaxNode> Nodes { get; }

	/// <inheritdoc/>
	public IReadOnlyList<TValue> Values { get; }

	/// <inheritdoc/>
	public IReadOnlyList<TSeparator> Separators { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="ConcreteSeparatedSyntaxList{TValue, TSeparator}"/> instance.</summary>
	/// <param name="nodes">All of the nodes in the list.</param>
	/// <param name="values">The value nodes in the list.</param>
	/// <param name="separators">The separator nodes in the list.</param>
	public ConcreteSeparatedSyntaxList(IReadOnlyList<IConcreteSyntaxNode> nodes, IReadOnlyList<TValue> values, IReadOnlyList<TSeparator> separators)
	{
		Nodes = nodes;
		Values = values;
		Separators = separators;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override IEnumerable<IConcreteSyntaxNode> GetChildren() => Nodes;
	#endregion
}

/// <summary>
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
public class ConcreteSeparatedSyntaxList<TValue> : ConcreteSeparatedSyntaxList<TValue, ITokenNode>
	where TValue : class, IConcreteSyntaxNode
{
	#region Constructors
	/// <summary>Creates a new <see cref="ConcreteSeparatedSyntaxList{TValue}"/> instance.</summary>
	/// <param name="nodes">All of the nodes in the list.</param>
	/// <param name="values">The value nodes in the list.</param>
	/// <param name="separators">The separator nodes in the list.</param>
	public ConcreteSeparatedSyntaxList(IReadOnlyList<IConcreteSyntaxNode> nodes, IReadOnlyList<TValue> values, IReadOnlyList<ITokenNode> separators) : base(nodes, values, separators)
	{
	}
	#endregion
}
