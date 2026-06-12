namespace OwlDomain.ParsingTools.Semantics.Syntax;

/// <summary>
/// 	Represents a semantic syntax list that has separator nodes.
/// </summary>
public interface ISemanticSeparatedSyntaxList : ISeparatedSyntaxList, ISemanticSyntaxList, ISemanticSyntaxNode
{
	#region Properties
	/// <summary>The list of all of the syntax nodes in the list.</summary>
	new IReadOnlyList<ISemanticSyntaxNode> Nodes { get; }
	IReadOnlyList<ISyntaxNode> ISeparatedSyntaxList.Nodes => Nodes;

	/// <summary>The nodes that are acting as separators.</summary>
	new IReadOnlyList<ISemanticSyntaxNode> Separators { get; }
	IReadOnlyList<ISyntaxNode> ISeparatedSyntaxList.Separators => Separators;
	#endregion
}

/// <summary>
/// 	Represents a semantic syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
public interface ISemanticSeparatedSyntaxList<out TValue> : ISemanticSeparatedSyntaxList, ISeparatedSyntaxList<TValue>, ISemanticSyntaxList<TValue>
	where TValue : class, ISemanticSyntaxNode
{
}

/// <summary>
/// 	Represents a semantic syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
/// <typeparam name="TSeparator">The type of the separator nodes.</typeparam>
public interface ISemanticSeparatedSyntaxList<out TValue, out TSeparator> : ISemanticSeparatedSyntaxList<TValue>, ISeparatedSyntaxList<TValue, TSeparator>
	where TValue : class, ISemanticSyntaxNode
	where TSeparator : class, ISemanticSyntaxNode
{
	#region Properties
	/// <summary>The nodes that are acting as separators.</summary>
	new IReadOnlyList<TSeparator> Separators { get; }
	IReadOnlyList<ISyntaxNode> ISeparatedSyntaxList.Separators => Separators;
	IReadOnlyList<ISemanticSyntaxNode> ISemanticSeparatedSyntaxList.Separators => Separators;
	#endregion
}

/// <summary>
/// 	Represents a semantic syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
/// <typeparam name="TSeparator">The type of the separator nodes.</typeparam>
public class SemanticSeparatedSyntaxList<TValue, TSeparator> : BaseSemanticSyntaxNode, ISemanticSeparatedSyntaxList<TValue, TSeparator>
	where TValue : class, ISemanticSyntaxNode
	where TSeparator : class, ISemanticSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public override SyntaxKind Kind => SyntaxKind.SeparatedSyntaxList;

	/// <inheritdoc/>
	public IReadOnlyList<ISemanticSyntaxNode> Nodes { get; }

	/// <inheritdoc/>
	public IReadOnlyList<TValue> Values { get; }

	/// <inheritdoc/>
	public IReadOnlyList<TSeparator> Separators { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="SemanticSeparatedSyntaxList{TValue, TSeparator}"/> instance.</summary>
	/// <param name="nodes">All of the nodes in the list.</param>
	/// <param name="values">The value nodes in the list.</param>
	/// <param name="separators">The separator nodes in the list.</param>
	public SemanticSeparatedSyntaxList(IReadOnlyList<ISemanticSyntaxNode> nodes, IReadOnlyList<TValue> values, IReadOnlyList<TSeparator> separators)
	{
		Nodes = nodes;
		Values = values;
		Separators = separators;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override IEnumerable<ISemanticSyntaxNode> GetChildren() => Nodes;
	#endregion
}

/// <summary>
/// 	Represents a semantic syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
public class SemanticSeparatedSyntaxList<TValue> : SemanticSeparatedSyntaxList<TValue, ISemanticSyntaxToken>
	where TValue : class, ISemanticSyntaxNode
{
	#region Constructors
	/// <summary>Creates a new <see cref="SemanticSeparatedSyntaxList{TValue}"/> instance.</summary>
	/// <param name="nodes">All of the nodes in the list.</param>
	/// <param name="values">The value nodes in the list.</param>
	/// <param name="separators">The separator nodes in the list.</param>
	public SemanticSeparatedSyntaxList(
		IReadOnlyList<ISemanticSyntaxNode> nodes,
		IReadOnlyList<TValue> values,
		IReadOnlyList<ISemanticSyntaxToken> separators)
		: base(nodes, values, separators)
	{
	}
	#endregion
}
