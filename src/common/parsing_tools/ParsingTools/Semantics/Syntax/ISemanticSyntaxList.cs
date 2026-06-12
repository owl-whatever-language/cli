namespace OwlDomain.ParsingTools.Semantics.Syntax;

/// <summary>
/// 	Represents a semantic syntax list.
/// </summary>
public interface ISemanticSyntaxList : ISyntaxList, ISemanticSyntaxNode
{
}

/// <summary>
/// 	Represents a semantic syntax list.
/// </summary>
/// <typeparam name="TValue">The type of the semantic syntax nodes that the list stores.</typeparam>
public interface ISemanticSyntaxList<out TValue> : ISyntaxList<TValue>, ISemanticSyntaxList
	where TValue : class, ISemanticSyntaxNode
{
}

/// <summary>
/// 	Represents a semantic syntax list.
/// </summary>
/// <typeparam name="TValue">The type of the semantic syntax nodes that the list stores.</typeparam>
public class SemanticSyntaxList<TValue> : BaseSemanticSyntaxNode, ISemanticSyntaxList<TValue>
	where TValue : class, ISemanticSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public override SyntaxKind Kind => SyntaxKind.SyntaxList;

	/// <inheritdoc/>
	public IReadOnlyList<TValue> Values { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="SemanticSyntaxList{TValue}"/> instance.</summary>
	/// <param name="values">The values to store in the list.</param>
	public SemanticSyntaxList(IReadOnlyList<TValue> values)
	{
		Guard.IsOrdered(values);

		Values = values;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override IEnumerable<TValue> GetChildren() => Values;
	#endregion
}
