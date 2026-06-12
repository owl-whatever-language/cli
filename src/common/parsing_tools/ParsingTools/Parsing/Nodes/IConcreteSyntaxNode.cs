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
