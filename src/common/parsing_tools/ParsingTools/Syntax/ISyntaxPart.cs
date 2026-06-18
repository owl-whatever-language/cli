namespace OwlDomain.ParsingTools.Syntax;

public interface ISyntaxPart : ISyntaxNode
{
	#region Properties
	SyntaxKind Kind { get; }
	string? Lexeme { get; }
	object? Value { get; }
	ClassificationKind? Classification { get; }
	#endregion
}
