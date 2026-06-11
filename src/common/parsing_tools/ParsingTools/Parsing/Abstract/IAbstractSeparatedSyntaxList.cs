namespace OwlDomain.ParsingTools.Parsing.Abstract;

/// <summary>
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
public interface IAbstractSeparatedSyntaxList : ISeparatedSyntaxList, IAbstractSyntaxList, IAbstractSyntaxNode
{
	#region Properties
	/// <summary>The list of all of the syntax nodes in the list.</summary>
	new IReadOnlyList<IAbstractSyntaxNode> Nodes { get; }
	IReadOnlyList<ISyntaxNode> ISeparatedSyntaxList.Nodes => Nodes;
	#endregion
}

/// <summary>
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
public interface IAbstractSeparatedSyntaxList<out TValue> : IAbstractSeparatedSyntaxList, ISeparatedSyntaxList<TValue>, IAbstractSyntaxList<TValue>
	where TValue : class, IAbstractSyntaxNode
{
}

/// <summary>
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
/// <typeparam name="TSeparator">The type of the separator nodes.</typeparam>
public interface IAbstractSeparatedSyntaxList<out TValue, out TSeparator> : IAbstractSeparatedSyntaxList<TValue>, ISeparatedSyntaxList<TValue, TSeparator>
	where TValue : class, IAbstractSyntaxNode
	where TSeparator : class, IAbstractSyntaxNode
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
/// <typeparam name="TConcrete">The type of the concrete syntax node that the abstract syntax node is modelled after.</typeparam>
public interface IAbstractSeparatedSyntaxList<out TValue, out TSeparator, out TConcrete> :
	IAbstractSeparatedSyntaxList<TValue>,
	ISeparatedSyntaxList<TValue, TSeparator>,
	IAbstractSyntaxNode<IConcreteSeparatedSyntaxList<TConcrete>>
	where TValue : class, IAbstractSyntaxNode
	where TSeparator : class, IAbstractSyntaxNode
	where TConcrete : class, IConcreteSyntaxNode
{
}

/// <summary>
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
/// <typeparam name="TSeparator">The type of the separator nodes.</typeparam>
/// <typeparam name="TConcrete">The type of the concrete syntax node that the abstract syntax node is modelled after.</typeparam>
public class AbstractSeparatedSyntaxList<TValue, TSeparator, TConcrete> :
	BaseAbstractSyntaxNode<IConcreteSeparatedSyntaxList<TConcrete>>,
	IAbstractSeparatedSyntaxList<TValue, TSeparator, TConcrete>
	where TValue : class, IAbstractSyntaxNode
	where TSeparator : class, IAbstractSyntaxNode
	where TConcrete : class, IConcreteSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public override SyntaxKind Kind => SyntaxKind.SeparatedSyntaxList;

	/// <inheritdoc/>
	public IReadOnlyList<IAbstractSyntaxNode> Nodes { get; }

	/// <inheritdoc/>
	public IReadOnlyList<TValue> Values { get; }

	/// <inheritdoc/>
	public IReadOnlyList<TSeparator> Separators { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="AbstractSeparatedSyntaxList{TValue, TSeparator, TConcrete}"/> instance.</summary>
	/// <param name="concrete">The concrete syntax tree that this abstract syntax node is modelled after.</param>
	/// <param name="nodes">All of the nodes in the list.</param>
	/// <param name="values">The value nodes in the list.</param>
	/// <param name="separators">The separator nodes in the list.</param>
	public AbstractSeparatedSyntaxList(
		IConcreteSeparatedSyntaxList<TConcrete> concrete,
		IReadOnlyList<IAbstractSyntaxNode> nodes,
		IReadOnlyList<TValue> values,
		IReadOnlyList<TSeparator> separators)
		: base(concrete)
	{
		Nodes = nodes;
		Values = values;
		Separators = separators;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override IEnumerable<IAbstractSyntaxNode> GetChildren() => Nodes;
	#endregion
}

/// <summary>
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
/// <typeparam name="TConcrete">The type of the concrete syntax node that the abstract syntax node is modelled after.</typeparam>
public class AbstractSeparatedSyntaxList<TValue, TConcrete> : AbstractSeparatedSyntaxList<TValue, IAbstractSyntaxToken, TConcrete>
	where TValue : class, IAbstractSyntaxNode
	where TConcrete : class, IConcreteSyntaxNode
{
	#region Constructors
	/// <summary>Creates a new <see cref="AbstractSeparatedSyntaxList{TValue, TConcrete}"/> instance.</summary>
	/// <param name="concrete">The concrete syntax tree that this abstract syntax node is modelled after.</param>
	/// <param name="nodes">All of the nodes in the list.</param>
	/// <param name="values">The value nodes in the list.</param>
	/// <param name="separators">The separator nodes in the list.</param>
	public AbstractSeparatedSyntaxList(
		IConcreteSeparatedSyntaxList<TConcrete> concrete,
		IReadOnlyList<IAbstractSyntaxNode> nodes,
		IReadOnlyList<TValue> values,
		IReadOnlyList<IAbstractSyntaxToken> separators)
		: base(concrete, nodes, values, separators)
	{
	}
	#endregion
}
