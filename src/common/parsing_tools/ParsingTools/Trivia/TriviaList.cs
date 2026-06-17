namespace OwlDomain.ParsingTools.Trivia;

public sealed class TriviaList : SyntaxList<ISyntaxTrivia>
{
	#region Properties
	public static TriviaList Empty { get; } = [];
	#endregion

	#region Constructors
	public TriviaList() { }
	public TriviaList(IReadOnlyList<ISyntaxTrivia> trivia) : base(trivia) { }
	#endregion
}
