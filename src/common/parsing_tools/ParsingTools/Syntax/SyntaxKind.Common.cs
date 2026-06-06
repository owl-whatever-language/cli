namespace OwlDomain.ParsingTools.Syntax;

partial struct SyntaxKind
{
	#region Properties
	/// <summary>Represents a syntax kind for white-space trivia.</summary>
	public static SyntaxKind WhiteSpace { get; } = new("white_space", SyntaxCategory.Trivia);

	/// <summary>Represents a syntax kind for comment trivia.</summary>
	public static SyntaxKind Comment { get; } = new("comment", SyntaxCategory.Trivia);
	#endregion
}
