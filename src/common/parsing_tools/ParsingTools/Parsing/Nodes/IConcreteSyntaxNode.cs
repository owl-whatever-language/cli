namespace OwlDomain.ParsingTools.Parsing.Nodes;

/// <summary>
/// 	Represents a syntax node in the concrete syntax tree (CST).
/// </summary>
public interface IConcreteSyntaxNode : ISyntaxNode<IConcreteSyntaxNode>
{
	#region Properties
	/// <summary>The full position that the syntax node takes up in the source.</summary>
	/// <remarks>The full position includes the leading and trailing trivia.</remarks>
	IndexedPositionRange FullPosition { get; }

	/// <summary>Whether the full syntax node is fabricated.</summary>
	bool IsFabricated { get; }
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a syntax node in the concrete syntax tree (CST).
/// </summary>
public abstract class BaseConcreteSyntaxNode : BaseSyntaxNode<IConcreteSyntaxNode>, IConcreteSyntaxNode
{
}

/// <summary>
/// 	Contains various extensions related to the <see cref="IConcreteSyntaxNode"/>.
/// </summary>
public static class IConcreteSyntaxNodeExtensions
{
	extension(IConcreteSyntaxNode node)
	{
		#region Methods
		/// <summary>Flattens the current node to all of the leaf tokens that make it up.</summary>
		/// <returns>A list of the leaf tokens that make up the current node.</returns>
		public IReadOnlyList<IConcreteSyntaxToken> Flatten() => node.Flatten<IConcreteSyntaxToken>();
		#endregion
	}
}