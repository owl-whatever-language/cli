namespace OwlDomain.ParsingTools.Semantics;

/// <summary>
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
public interface ISemanticSeparatedSyntaxList : ISeparatedSyntaxList, ISemanticSyntaxList, ISemanticSyntaxNode
{
	#region Properties
	/// <summary>The list of all of the syntax nodes in the list.</summary>
	new IReadOnlyList<ISemanticSyntaxNode> Nodes { get; }
	IReadOnlyList<ISyntaxNode> ISeparatedSyntaxList.Nodes => Nodes;
	#endregion
}

/// <summary>
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
public interface ISemanticSeparatedSyntaxList<out TValue> : ISemanticSeparatedSyntaxList, ISeparatedSyntaxList<TValue>, ISemanticSyntaxList<TValue>
	where TValue : class, ISemanticSyntaxNode
{
}

/// <summary>
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
/// <typeparam name="TSeparator">The type of the separator nodes.</typeparam>
public interface ISemanticSeparatedSyntaxList<out TValue, out TSeparator> : ISemanticSeparatedSyntaxList<TValue>, ISeparatedSyntaxList<TValue, TSeparator>
	where TValue : class, ISemanticSyntaxNode
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
/// <typeparam name="TAbstract">The type of the abstract syntax node that the semantic syntax node is modelled after.</typeparam>
public interface ISemanticSeparatedSyntaxList<out TValue, out TSeparator, out TAbstract> :
	ISemanticSeparatedSyntaxList<TValue>,
	ISeparatedSyntaxList<TValue, TSeparator>,
	ISemanticSyntaxNode<IAbstractSeparatedSyntaxList<TAbstract>>
	where TValue : class, ISemanticSyntaxNode
	where TSeparator : class, ISyntaxNode
	where TAbstract : class, IAbstractSyntaxNode
{
}

/// <summary>
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
/// <typeparam name="TSeparator">The type of the separator nodes.</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax node that the semantic syntax node is modelled after.</typeparam>
public class SemanticSeparatedSyntaxList<TValue, TSeparator, TAbstract> :
	BaseSemanticSyntaxNode<IAbstractSeparatedSyntaxList<TAbstract>>,
	ISemanticSeparatedSyntaxList<TValue, TSeparator, TAbstract>
	where TValue : class, ISemanticSyntaxNode
	where TSeparator : class, ISyntaxNode
	where TAbstract : class, IAbstractSyntaxNode
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
	/// <summary>Creates a new <see cref="SemanticSeparatedSyntaxList{TValue, TSeparator, TAbstract}"/> instance.</summary>
	/// <param name="abstract">The abstract syntax tree that this semantic syntax node is modelled after.</param>
	/// <param name="nodes">All of the nodes in the list.</param>
	/// <param name="values">The value nodes in the list.</param>
	/// <param name="separators">The separator nodes in the list.</param>
	public SemanticSeparatedSyntaxList(
		IAbstractSeparatedSyntaxList<TAbstract> @abstract,
		IReadOnlyList<ISemanticSyntaxNode> nodes,
		IReadOnlyList<TValue> values,
		IReadOnlyList<TSeparator> separators)
		: base(@abstract)
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
/// 	Represents a general syntax list that has separator nodes.
/// </summary>
/// <typeparam name="TValue">The type of the value nodes.</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax node that the semantic syntax node is modelled after.</typeparam>
public class SemanticSeparatedSyntaxList<TValue, TAbstract> : SemanticSeparatedSyntaxList<TValue, ITokenNode, TAbstract>
	where TValue : class, ISemanticSyntaxNode
	where TAbstract : class, IAbstractSyntaxNode
{
	#region Constructors
	/// <summary>Creates a new <see cref="SemanticSeparatedSyntaxList{TValue, TAbstract}"/> instance.</summary>
	/// <param name="abstract">The abstract syntax tree that this semantic syntax node is modelled after.</param>
	/// <param name="nodes">All of the nodes in the list.</param>
	/// <param name="values">The value nodes in the list.</param>
	/// <param name="separators">The separator nodes in the list.</param>
	public SemanticSeparatedSyntaxList(
		IAbstractSeparatedSyntaxList<TAbstract> @abstract,
		IReadOnlyList<ISemanticSyntaxNode> nodes,
		IReadOnlyList<TValue> values,
		IReadOnlyList<ITokenNode> separators)
		: base(@abstract, nodes, values, separators)
	{
	}
	#endregion
}
