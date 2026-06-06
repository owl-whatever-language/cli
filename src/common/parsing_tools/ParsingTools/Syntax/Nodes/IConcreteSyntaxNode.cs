namespace OwlDomain.ParsingTools.Syntax.Nodes;

/// <summary>
/// 	Represents a syntax node in the concrete syntax tree (CST).
/// </summary>
public interface IConcreteSyntaxNode : ISyntaxNode
{
	#region Properties
	/// <summary>The full position that the syntax node takes up in the source.</summary>
	/// <remarks>The full position includes the leading and trailing trivia.</remarks>
	IndexedPositionRange FullPosition { get; }
	#endregion
}
