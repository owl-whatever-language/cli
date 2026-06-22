namespace OwlDomain.ParsingTools;

public static class MarkupExtensions
{
	extension(AnsiMarkupSegment segment)
	{
		#region Methods
		public Markup ToMarkup()
		{
			string text = segment.ToString();
			return new(text);
		}
		#endregion
	}
	extension(IEnumerable<AnsiMarkupSegment> segments)
	{
		#region Methods
		public Markup ToMarkup() => segments.ToArray().ToMarkup();
		#endregion
	}
	extension(IReadOnlyList<AnsiMarkupSegment> segments)
	{
		#region Methods
		public Markup ToMarkup()
		{
			string text = string.Concat(segments);
			return new(text);
		}
		#endregion
	}
	extension(AnsiMarkupSegment[] segments)
	{
		#region Methods
		public Markup ToMarkup()
		{
			string text = string.Concat(segments);
			return new(text);
		}
		#endregion
	}
}
