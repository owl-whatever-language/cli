namespace OwlDomain.ParsingTools.Text;

public readonly struct TextFragment
{
	#region Properties
	public string Text { get; }
	public ClassificationKind? Classification { get; }
	#endregion

	#region Constructors
	public TextFragment(string text, ClassificationKind? classification = null)
	{
		Text = text;
		Classification = classification;
	}
	#endregion
}

public sealed class TextFragmentCollection : IReadOnlyList<TextFragment>
{
	#region Fields
	private readonly IReadOnlyList<TextFragment> _fragments;
	#endregion

	#region Properties
	public int Count => _fragments.Count;
	public bool IsOnlyLineBreak => Count is 1 && _fragments[0].Classification == ClassificationKind.LineBreak;
	#endregion

	#region Indexers
	public TextFragment this[int index] => _fragments[index];
	#endregion

	#region Constructors
	public TextFragmentCollection(params IReadOnlyList<TextFragment> fragments) => _fragments = fragments;
	#endregion

	#region Methods
	public IEnumerator<TextFragment> GetEnumerator() => _fragments.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	#endregion
}

public static class TextFragmentExtensions
{
	extension(IEnumerable<TextFragment> fragments)
	{
		#region Methods
		public IReadOnlyList<TextFragmentCollection> ToLines(bool trimLastLines = true, bool ensureOneLine = true, bool includeLineBreak = false)
		{
			List<TextFragmentCollection> lines = [];
			List<TextFragment> current = [];

			foreach (TextFragment fragment in fragments)
			{
				if (fragment.Classification == ClassificationKind.LineBreak)
				{
					if (includeLineBreak)
						current.Add(fragment);

					TextFragmentCollection line = new(current);
					lines.Add(line);
					current = [];
				}
				else
					current.Add(fragment);
			}

			if (current.Any())
			{
				TextFragmentCollection line = new(current);
				lines.Add(line);
			}

			if (ensureOneLine && lines.Count is 0)
			{
				TextFragment fragment = new("\n", ClassificationKind.LineBreak);
				lines.Add(new(fragment));
				return lines;
			}

			if (trimLastLines is false)
				return lines;

			while (lines.Last().IsOnlyLineBreak)
			{
				if (ensureOneLine && lines.Count is 1)
					break;

				lines.RemoveAt(lines.Count - 1);
			}

			return lines;
		}
		#endregion
	}
}
