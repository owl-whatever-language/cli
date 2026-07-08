namespace OwlDomain.ParsingTools.Text.Fragments;

public readonly struct TextFragment
{
	#region Properties
	public string Text { get; }
	public ISyntaxPart? Syntax { get; }
	public IReadOnlyList<ClassificationKind> Classifications { get; }
	public ClassificationKind? Classification => Classifications.FirstOrDefault();
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
	public bool IsComment => Classification.IsMatch(ClassificationKind.Comment);
	#endregion

	#region Constructors
	public TextFragment(string text, ClassificationKind? classification = null, ISyntaxPart? syntax = null)
	{
		Text = text;
		Syntax = syntax;

		Classifications = classification is not null ? [classification.Value] : [];
	}
	public TextFragment(string text, ISyntaxPart? syntax, params IReadOnlyList<ClassificationKind>? classifications)
	{
		Text = text;
		Syntax = syntax;

		Classifications = classifications ?? [];
	}
	#endregion

	#region Functions
	public static TextFragmentLineCollection LineBuilder(Action<TextFragmentLineCollection> callback)
	{
		TextFragmentLineCollection lines = [];
		callback.Invoke(lines);

		return lines;
	}
	#endregion

	#region Methods
	public TextFragment With(ClassificationKind alternate) => new(Text, Syntax, [.. Classifications, alternate]);
	public override string ToString() => Text;
	#endregion
}
