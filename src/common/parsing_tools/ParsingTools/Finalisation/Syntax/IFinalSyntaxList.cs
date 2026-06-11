namespace OwlDomain.ParsingTools.Finalisation.Syntax;

/// <summary>
/// 	Represents a general syntax list.
/// </summary>
public interface IFinalSyntaxList : ISyntaxList, IFinalSyntaxNode
{
	#region Properties
	/// <summary>The values stored in the syntax list.</summary>
	new IReadOnlyList<IFinalSyntaxNode> Values { get; }
	IReadOnlyList<ISyntaxNode> ISyntaxList.Values => Values;
	#endregion
}

/// <summary>
/// 	Represents a general syntax list.
/// </summary>
/// <typeparam name="TValue">The type of the syntax nodes that the list stores.</typeparam>
public interface IFinalSyntaxList<out TValue> : ISyntaxList<TValue>, IFinalSyntaxList
	where TValue : class, IFinalSyntaxNode
{
	#region Properties
	/// <summary>The values stored in the syntax list.</summary>
	new IReadOnlyList<TValue> Values { get; }
	IReadOnlyList<ISyntaxNode> ISyntaxList.Values => Values;
	IReadOnlyList<IFinalSyntaxNode> IFinalSyntaxList.Values => Values;
	#endregion
}

/// <summary>
/// 	Represents a general syntax list.
/// </summary>
/// <typeparam name="TValue">The type of the syntax nodes that the list stores.</typeparam>
/// <typeparam name="TSemantic">The type of the semantic syntax node that the final syntax node is modelled after.</typeparam>
public interface IFinalSyntaxList<out TValue, out TSemantic> : IFinalSyntaxList<TValue>, IFinalSyntaxNode<ISemanticSyntaxList<TSemantic>>
	where TValue : class, IFinalSyntaxNode
	where TSemantic : class, ISemanticSyntaxNode
{
}

/// <summary>
/// 	Represents a general syntax list.
/// </summary>
/// <typeparam name="TValue">The type of the syntax nodes that the list stores.</typeparam>
/// <typeparam name="TSemantic">The type of the semantic syntax node that the final syntax node is modelled after.</typeparam>
public class FinalSyntaxList<TValue, TSemantic> : BaseFinalSyntaxNode<ISemanticSyntaxList<TSemantic>>, IFinalSyntaxList<TValue, TSemantic>
	where TValue : class, IFinalSyntaxNode
	where TSemantic : class, ISemanticSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public override SyntaxKind Kind => SyntaxKind.SyntaxList;

	/// <inheritdoc/>
	public IReadOnlyList<TValue> Values { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="FinalSyntaxList{TValue, TSemantic}"/> instance.</summary>
	/// <param name="semantic">The semantic syntax tree that this final syntax node is modelled after.</param>
	/// <param name="values">The value nodes stored in the list.</param>
	public FinalSyntaxList(ISemanticSyntaxList<TSemantic> semantic, IReadOnlyList<TValue> values) : base(semantic)
	{
		Values = values;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override IEnumerable<IFinalSyntaxNode> GetChildren() => Values;
	#endregion
}
