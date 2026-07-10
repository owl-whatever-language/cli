namespace OwlDomain.ParsingTools.Text.Fragments;

public readonly struct TextFragment
{
	#region Properties
	public string Text => field ?? "";
	public ISyntaxPart? Syntax { get; }
	public IReadOnlyList<ClassificationKind> Classifications => field ?? [];
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
	public bool IsSourcePrefix
	{
		get
		{
			return
				Classification == ClassificationKind.LineNumber ||
				Classification == ClassificationKind.Margin;
		}
	}
	public bool IsWhiteSpaceOrSourcePrefix => IsWhitespace || IsSourcePrefix;
	public bool IsComment => Classification.IsMatch(ClassificationKind.Comment);
	public bool IsDimmed => Classifications.Contains(ClassificationKind.Dim);
	public bool IsSnipped => Classifications.Contains(ClassificationKind.Snipped);
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
