namespace OwlDomain.ParsingTools.Syntax;

public static class StyledSyntaxNodeExtensions
{
	extension(ISyntaxNode node)
	{
		#region Methods
		public IReadOnlyList<AnsiMarkupSegment> Style(IClassificationStyles styles, bool includeStartAndEndTrivia = true)
		{
			List<AnsiMarkupSegment> styled = [];

			foreach (ISyntaxPart part in node.ToParts(includeStartAndEndTrivia))
			{
				if (part.Lexeme is null)
					continue;

				AnsiMarkupSegment segment = new(
					part.Lexeme,
					styles.GetStyle(part.Classification),
					null);

				styled.Add(segment);
			}

			return styled;
		}
		public Markup StyleMarkup(IClassificationStyles styles, bool includeStartAndEndTrivia = true)
		{
			List<AnsiMarkupSegment> styled = [];

			foreach (ISyntaxPart part in node.ToParts(includeStartAndEndTrivia))
			{
				if (part.Lexeme is null)
					continue;

				AnsiMarkupSegment segment = new(
					part.Lexeme,
					styles.GetStyle(part.Classification),
					null);

				styled.Add(segment);
			}

			string text = string.Concat(styled);
			return new(text);
		}
		public Rows StyledSource(IClassificationStyles styles, bool includeLineNumbers = true, Style? marginStyle = null)
		{
			marginStyle ??= new(Color.Gray);
			List<Markup> rows = [];

			int startLine = node.FullPosition.Start.Line;
			IReadOnlyList<TextFragmentCollection> lines = node.ToTextFragments().ToLines();
			int maxLineNumberLength = includeLineNumbers ? (startLine + lines.Count).ToString("n0").Length : 0;

			for (int i = 0; i < lines.Count; i++)
			{
				TextFragmentCollection line = lines[i];

				string marginText = includeLineNumbers ? $" {(startLine + i).ToString("n0").PadLeft(maxLineNumberLength)} | " : " | ";

				/* // Disabled for now because I'm not sure if I want it, it adds missing
				   // indentation when the start node isn't at the start of a line.
				if (i == 0 && node.FullPosition.IsMultiline)
				{
					StringBuilder missingIndent = new();
					missingIndent.Append(' ', node.FullPosition.Start.Column - 1);
					marginText += missingIndent.ToString();
				}
				*/

				AnsiMarkupSegment margin = new(marginText, marginStyle.Value, link: null);

				AnsiMarkupSegment[] segments = [margin, .. line.Style(styles)];
				string markup = string.Concat(segments);
				rows.Add(new(markup));
			}

			return new Rows(rows);
		}
		public Panel StyledSourcePanel(ISourceFile source, IClassificationStyles styles, bool includeLineNumbers = true, Style? marginStyle = null)
		{
			Rows rows = StyledSource(node, styles, includeLineNumbers, marginStyle);
			Padder padder = new(rows);
			Panel panel = new Panel(padder).Header(source.SimpleName);

			return panel;
		}
		#endregion

	}
}
