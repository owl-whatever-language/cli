namespace OwlDomain.ParsingTools.Parsing.Abstract;

/// <summary>
/// 	Represents a general syntax list.
/// </summary>
public interface IAbstractSyntaxList : ISyntaxList, IAbstractSyntaxNode
{
	#region Properties
	/// <summary>The values stored in the syntax list.</summary>
	new IReadOnlyList<IAbstractSyntaxNode> Values { get; }
	IReadOnlyList<ISyntaxNode> ISyntaxList.Values => Values;
	#endregion
}

/// <summary>
/// 	Represents a general syntax list.
/// </summary>
/// <typeparam name="TValue">The type of the syntax nodes that the list stores.</typeparam>
public interface IAbstractSyntaxList<out TValue> : ISyntaxList<TValue>, IAbstractSyntaxList
	where TValue : class, IAbstractSyntaxNode
{
	#region Properties
	/// <summary>The values stored in the syntax list.</summary>
	new IReadOnlyList<TValue> Values { get; }
	IReadOnlyList<ISyntaxNode> ISyntaxList.Values => Values;
	IReadOnlyList<IAbstractSyntaxNode> IAbstractSyntaxList.Values => Values;
	#endregion
}

/// <summary>
/// 	Represents a general syntax list.
/// </summary>
/// <typeparam name="TValue">The type of the syntax nodes that the list stores.</typeparam>
/// <typeparam name="TConcrete">The type of the concrete syntax node that the abstract syntax node is modelled after.</typeparam>
public interface IAbstractSyntaxList<out TValue, out TConcrete> : IAbstractSyntaxList<TValue>, IAbstractSyntaxNode<IConcreteSyntaxList<TConcrete>>
	where TValue : class, IAbstractSyntaxNode
	where TConcrete : class, IConcreteSyntaxNode
{
}

/// <summary>
/// 	Represents a general syntax list.
/// </summary>
/// <typeparam name="TValue">The type of the syntax nodes that the list stores.</typeparam>
/// <typeparam name="TConcrete">The type of the concrete syntax node that the abstract syntax node is modelled after.</typeparam>
public class AbstractSyntaxList<TValue, TConcrete> : BaseAbstractSyntaxNode<IConcreteSyntaxList<TConcrete>>, IAbstractSyntaxList<TValue, TConcrete>
	where TValue : class, IAbstractSyntaxNode
	where TConcrete : class, IConcreteSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public override SyntaxKind Kind => SyntaxKind.SyntaxList;

	/// <inheritdoc/>
	public IReadOnlyList<TValue> Values { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="AbstractSyntaxList{TValue, TConcrete}"/> instance.</summary>
	/// <param name="concrete">The concrete syntax tree that this abstract syntax node is modelled after.</param>
	/// <param name="values">The value nodes stored in the list.</param>
	public AbstractSyntaxList(IConcreteSyntaxList<TConcrete> concrete, IReadOnlyList<TValue> values) : base(concrete)
	{
		Values = values;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override IEnumerable<IAbstractSyntaxNode> GetChildren() => Values;
	#endregion
}
