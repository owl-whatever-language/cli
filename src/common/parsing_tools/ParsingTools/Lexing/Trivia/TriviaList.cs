namespace OwlDomain.ParsingTools.Lexing.Trivia;

/// <summary>
/// 	Represents a list of trivia nodes in the concrete syntax tree (CST).
/// </summary>
public sealed class TriviaList : ConcreteSyntaxList<ITriviaNode>, IConcreteSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public override SyntaxKind Kind => SyntaxKind.TriviaList;
	#endregion

	#region Constructors
	/// <summary>Creates a new, empty trivia list.</summary>
	public TriviaList() : base() { }

	/// <summary>Creates a new trivia list.</summary>
	/// <param name="nodes">The nodes that are a part of the trivia list.</param>
	/// <exception cref="ArgumentException">Thrown if the given list of trivia <paramref name="nodes"/> is not ordered.</exception>
	public TriviaList(IReadOnlyList<ITriviaNode> nodes) : base(nodes)
	{
	}
	#endregion
}
