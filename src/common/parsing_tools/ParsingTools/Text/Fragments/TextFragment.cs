namespace OwlDomain.ParsingTools.Text.Fragments;

public readonly struct TextFragment
{
	#region Properties
	public string Text { get; }
	public ClassificationKind? Classification { get; }
	public ISyntaxPart? Syntax { get; }
	public bool IsWhitespace
	{
		get
		{
			if (Classification is null)
				return Text.IsWhiteSpace();

			return
				Classification == ClassificationKind.Whitespace ||
				Classification == ClassificationKind.Indentation ||
				Classification == ClassificationKind.LineBreak;
		}
	}
	#endregion

	#region Constructors
	public TextFragment(string text, ClassificationKind? classification = null, ISyntaxPart? syntax = null)
	{
		Text = text;
		Classification = classification;
		Syntax = syntax;
	}
	#endregion

	#region Methods
	public override string ToString() => Text;
	#endregion
}
