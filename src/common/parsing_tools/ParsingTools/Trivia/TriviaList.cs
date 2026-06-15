namespace OwlDomain.ParsingTools.Trivia;

public sealed class TriviaList : SyntaxList<ITriviaNode>
{
	#region Constructors
	public TriviaList() { }
	public TriviaList(IReadOnlyList<ITriviaNode> trivia) : base(trivia) { }
	#endregion
}
