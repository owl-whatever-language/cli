namespace OwlDomain.ParsingTools.Finalisation.Syntax;

/// <summary>
/// 	Represents a final syntax list that has separator nodes.
/// </summary>
public interface IFinalSeparatedSyntaxList : ISeparatedSyntaxList, IFinalSyntaxList, IFinalSyntaxNode
{
	#region Properties
	/// <summary>The list of all of the syntax nodes in the list.</summary>
	new IReadOnlyList<IFinalSyntaxNode> Nodes { get; }
	IReadOnlyList<ISyntaxNode> ISeparatedSyntaxList.Nodes => Nodes;

	/// <summary>The nodes that are acting as separators.</summary>
	new IReadOnlyList<IFinalSyntaxNode> Separators { get; }
	IReadOnlyList<ISyntaxNode> ISeparatedSyntaxList.Separators => Separators;
	#endregion
}

/// <summary>
/// 	Represents a final syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
public interface IFinalSeparatedSyntaxList<out TValue> : IFinalSeparatedSyntaxList, ISeparatedSyntaxList<TValue>, IFinalSyntaxList<TValue>
	where TValue : class, IFinalSyntaxNode
{
}

/// <summary>
/// 	Represents a final syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
/// <typeparam name="TSeparator">The type of the separator nodes.</typeparam>
public interface IFinalSeparatedSyntaxList<out TValue, out TSeparator> : IFinalSeparatedSyntaxList<TValue>, ISeparatedSyntaxList<TValue, TSeparator>
	where TValue : class, IFinalSyntaxNode
	where TSeparator : class, IFinalSyntaxNode
{
	#region Properties
	/// <summary>The nodes that are acting as separators.</summary>
	new IReadOnlyList<TSeparator> Separators { get; }
	IReadOnlyList<ISyntaxNode> ISeparatedSyntaxList.Separators => Separators;
	IReadOnlyList<IFinalSyntaxNode> IFinalSeparatedSyntaxList.Separators => Separators;
	#endregion
}

/// <summary>
/// 	Represents a final syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
/// <typeparam name="TSeparator">The type of the separator nodes.</typeparam>
public class FinalSeparatedSyntaxList<TValue, TSeparator> : BaseFinalSyntaxNode, IFinalSeparatedSyntaxList<TValue, TSeparator>
	where TValue : class, IFinalSyntaxNode
	where TSeparator : class, IFinalSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public override SyntaxKind Kind => SyntaxKind.SeparatedSyntaxList;

	/// <inheritdoc/>
	public IReadOnlyList<IFinalSyntaxNode> Nodes { get; }

	/// <inheritdoc/>
	public IReadOnlyList<TValue> Values { get; }

	/// <inheritdoc/>
	public IReadOnlyList<TSeparator> Separators { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="FinalSeparatedSyntaxList{TValue, TSeparator}"/> instance.</summary>
	/// <param name="nodes">All of the nodes in the list.</param>
	/// <param name="values">The value nodes in the list.</param>
	/// <param name="separators">The separator nodes in the list.</param>
	public FinalSeparatedSyntaxList(IReadOnlyList<IFinalSyntaxNode> nodes, IReadOnlyList<TValue> values, IReadOnlyList<TSeparator> separators)
	{
		Nodes = nodes;
		Values = values;
		Separators = separators;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override IEnumerable<IFinalSyntaxNode> GetChildren() => Nodes;
	#endregion
}

/// <summary>
/// 	Represents a final syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
public class FinalSeparatedSyntaxList<TValue> : FinalSeparatedSyntaxList<TValue, IFinalSyntaxToken>
	where TValue : class, IFinalSyntaxNode
{
	#region Constructors
	/// <summary>Creates a new <see cref="FinalSeparatedSyntaxList{TValue}"/> instance.</summary>
	/// <param name="nodes">All of the nodes in the list.</param>
	/// <param name="values">The value nodes in the list.</param>
	/// <param name="separators">The separator nodes in the list.</param>
	public FinalSeparatedSyntaxList(
		IReadOnlyList<IFinalSyntaxNode> nodes,
		IReadOnlyList<TValue> values,
		IReadOnlyList<IFinalSyntaxToken> separators)
		: base(nodes, values, separators)
	{
	}
	#endregion
}
