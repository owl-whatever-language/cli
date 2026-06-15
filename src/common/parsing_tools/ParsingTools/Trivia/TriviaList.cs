namespace OwlDomain.ParsingTools.Trivia;

public sealed class TriviaList : SyntaxList<ITriviaNode>
{
	#region Properties
	public static TriviaList Empty { get; } = [];
	#endregion

	#region Constructors
	public TriviaList() { }
	public TriviaList(IReadOnlyList<ITriviaNode> trivia) : base(trivia) { }
	#endregion
}
