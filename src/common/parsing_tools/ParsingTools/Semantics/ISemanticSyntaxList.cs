namespace OwlDomain.ParsingTools.Semantics;

/// <summary>
/// 	Represents a general syntax list.
/// </summary>
public interface ISemanticSyntaxList : ISyntaxList, ISemanticSyntaxNode
{
	#region Properties
	/// <summary>The values stored in the syntax list.</summary>
	new IReadOnlyList<ISemanticSyntaxNode> Values { get; }
	IReadOnlyList<ISyntaxNode> ISyntaxList.Values => Values;
	#endregion
}

/// <summary>
/// 	Represents a general syntax list.
/// </summary>
/// <typeparam name="TValue">The type of the syntax nodes that the list stores.</typeparam>
public interface ISemanticSyntaxList<out TValue> : ISyntaxList<TValue>, ISemanticSyntaxList
	where TValue : class, ISemanticSyntaxNode
{
	#region Properties
	/// <summary>The values stored in the syntax list.</summary>
	new IReadOnlyList<TValue> Values { get; }
	IReadOnlyList<ISyntaxNode> ISyntaxList.Values => Values;
	IReadOnlyList<ISemanticSyntaxNode> ISemanticSyntaxList.Values => Values;
	#endregion
}

/// <summary>
/// 	Represents a general syntax list.
/// </summary>
/// <typeparam name="TValue">The type of the syntax nodes that the list stores.</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax node that the semantic syntax node is modelled after.</typeparam>
public interface ISemanticSyntaxList<out TValue, out TAbstract> : ISemanticSyntaxList<TValue>, ISemanticSyntaxNode<IAbstractSyntaxList<TAbstract>>
	where TValue : class, ISemanticSyntaxNode
	where TAbstract : class, IAbstractSyntaxNode
{
}

/// <summary>
/// 	Represents a general syntax list.
/// </summary>
/// <typeparam name="TValue">The type of the syntax nodes that the list stores.</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax node that the semantic syntax node is modelled after.</typeparam>
public class SemanticSyntaxList<TValue, TAbstract> : BaseSemanticSyntaxNode<IAbstractSyntaxList<TAbstract>>, ISemanticSyntaxList<TValue, TAbstract>
	where TValue : class, ISemanticSyntaxNode
	where TAbstract : class, IAbstractSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public override SyntaxKind Kind => SyntaxKind.SyntaxList;

	/// <inheritdoc/>
	public IReadOnlyList<TValue> Values { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="SemanticSyntaxList{TValue, TAbstract}"/> instance.</summary>
	/// <param name="abstract">The abstract syntax tree that this semantic syntax node is modelled after.</param>
	/// <param name="values">The value nodes stored in the list.</param>
	public SemanticSyntaxList(IAbstractSyntaxList<TAbstract> @abstract, IReadOnlyList<TValue> values) : base(@abstract)
	{
		Values = values;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override IEnumerable<ISemanticSyntaxNode> GetChildren() => Values;
	#endregion
}
