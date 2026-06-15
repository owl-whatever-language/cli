namespace OwlDomain.ParsingTools.Syntax;

public interface ISyntaxToken : ISyntaxPart
{
	#region Properties
	TriviaList LeadingTrivia { get; }
	TriviaList TrailingTrivia { get; }
	#endregion
}
