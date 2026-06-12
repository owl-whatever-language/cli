namespace OwlDomain.ParsingTools.Finalisation.Syntax;

/// <summary>
/// 	Represents a final syntax list.
/// </summary>
public interface IFinalSyntaxList : ISyntaxList, IFinalSyntaxNode
{
}

/// <summary>
/// 	Represents a final syntax list.
/// </summary>
/// <typeparam name="TValue">The type of the final syntax nodes that the list stores.</typeparam>
public interface IFinalSyntaxList<out TValue> : ISyntaxList<TValue>, IFinalSyntaxList
	where TValue : class, IFinalSyntaxNode
{
}

/// <summary>
/// 	Represents a final syntax list.
/// </summary>
/// <typeparam name="TValue">The type of the final syntax nodes that the list stores.</typeparam>
public class FinalSyntaxList<TValue> : BaseFinalSyntaxNode, IFinalSyntaxList<TValue>
	where TValue : class, IFinalSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public override SyntaxKind Kind => SyntaxKind.SyntaxList;

	/// <inheritdoc/>
	public IReadOnlyList<TValue> Values { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="FinalSyntaxList{TValue}"/> instance.</summary>
	/// <param name="values">The values to store in the list.</param>
	public FinalSyntaxList(IReadOnlyList<TValue> values)
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
