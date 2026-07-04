namespace OwlDomain.ParsingTools.Text;

public static class StyledTextFragmentExtensions
{
	extension(TextFragment fragment)
	{
		#region Methods
		public AnsiMarkupSegment Style(IClassificationStyling styling)
		{
			return new(
				// I have to convert to space because terminal output is weird with tabs.
				// I'm not just doing this because I like seeing 3 spaces.
				fragment.Text.Replace("\t", "   "),
				styling.Get(fragment.Classification).AsSpectre,
				null);
		}
		#endregion
	}

	extension(IEnumerable<TextFragment> fragments)
	{
		#region Methods
		public IEnumerable<AnsiMarkupSegment> Style(IClassificationStyling styling)
		{
			foreach (TextFragment fragment in fragments)
				yield return fragment.Style(styling);
		}
		public Markup StyleMarkup(IClassificationStyling styles)
		{
			AnsiMarkupSegment[] segments = Style(fragments, styles).ToArray();
			string text = string.Concat(segments);

			return new(text);
		}
		#endregion
	}

	extension(IReadOnlyList<TextFragment> fragments)
	{
		#region Methods
		public IReadOnlyList<AnsiMarkupSegment> Style(IClassificationStyling styles)
		{
			AnsiMarkupSegment[] styled = new AnsiMarkupSegment[fragments.Count];

			for (int i = 0; i < styled.Length; i++)
				styled[i] = fragments[i].Style(styles);

			return styled;
		}
		public Markup StyleMarkup(IClassificationStyling styles)
		{
			AnsiMarkupSegment[] styled = new AnsiMarkupSegment[fragments.Count];

			for (int i = 0; i < styled.Length; i++)
				styled[i] = fragments[i].Style(styles);

			string text = string.Concat(styled);
			return new(text);
		}
		#endregion
	}
	extension(IReadOnlyList<ITextFragmentLine> lines)
	{
		#region Methods
		public Rows Style(IClassificationStyling styles)
		{
			List<Markup> markups = [];

			foreach (ITextFragmentLine line in lines)
			{
				Markup markup = line.StyleMarkup(styles);
				markups.Add(markup);
			}

			return new(markups);
		}
		#endregion
	}
}
