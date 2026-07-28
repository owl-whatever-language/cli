namespace OwlDomain.ParsingTools.Sources;

public enum PositionKind
{
	Grapheme,
	Utf16,
}

public interface IPositionTranslator
{
	#region Methods
	LinePosition Convert(LinePosition position, PositionKind from, PositionKind to);
	#endregion
}

public static class PositionTranslatorExtensions
{
	extension(IPositionTranslator translator)
	{
		#region Methods
		public LinePosition Convert(IndexedLinePosition position, PositionKind from, PositionKind to)
		{
			return translator.Convert(position.Position, from, to);
		}
		public PositionRange Convert(PositionRange position, PositionKind from, PositionKind to)
		{
			LinePosition start = translator.Convert(position.Start, from, to);
			LinePosition end = translator.Convert(position.End, from, to);

			return new(start, end);
		}
		public PositionRange Convert(IndexedPositionRange position, PositionKind from, PositionKind to)
		{
			LinePosition start = translator.Convert(position.Start.Position, from, to);
			LinePosition end = translator.Convert(position.End.Position, from, to);

			return new(start, end);
		}
		#endregion
	}
}

public sealed class PositionTranslator : IPositionTranslator
{
	#region Nested types
	private readonly struct LineInfo(int[] graphemeToUtf16, int[] utf16ToGrapheme)
	{
		#region Properties
		public readonly int[] GraphemeToUtf16 = graphemeToUtf16;
		public readonly int[] Utf16ToGrapheme = utf16ToGrapheme;
		#endregion
	}
	#endregion

	#region Fields
	private readonly LineInfo?[] _info;
	#endregion

	#region Constructors
	public PositionTranslator(string text)
	{
		_info = CalculateInfo(text);
	}
	#endregion

	#region Methods
	public LinePosition Convert(LinePosition position, PositionKind from, PositionKind to)
	{
		if (from == to)
			return position;

		if (_info.Length is 0)
			return new(1, 1);

		int lineIndex = position.Line - 1;

		if (lineIndex < 0 || lineIndex > _info.Length - 1)
		{
			ThrowHelper.ThrowArgumentException(nameof(position), $"The given position ({position}) was on an invalid line. Highest available line is {_info.Length}.");
			return default;
		}

		LineInfo? line = _info[lineIndex];

		int[]? target = (from, to) switch
		{
			(PositionKind.Grapheme, PositionKind.Utf16) => line?.GraphemeToUtf16,
			(PositionKind.Utf16, PositionKind.Grapheme) => line?.Utf16ToGrapheme,

			_ => ThrowHelper.ThrowArgumentException<int[]>(nameof(from), $"Unsupported conversion combination ({from}) -> ({to}).")
		};

		if (target is null)
			return position;

		int columnIndex = Math.Min(position.Column - 1, target.Length - 1);
		int column;

		if (columnIndex < 0)
			column = 1;
		else if (columnIndex is 0 && target.Length is 0)
			column = 1;
		else
			column = target[columnIndex];

		return new(position.Line, column);
	}
	#endregion

	#region Helpers
	private static LineInfo?[] CalculateInfo(string text)
	{
		List<LineInfo?> lines = [];

		List<int> toGrapheme = [];
		List<int> toUtf16 = [];

		int graphemeColumn = 1;
		int utf16Column = 1;

		void MakeLine()
		{
			if (toUtf16.Count == toGrapheme.Count)
			{
				// Note(Nightowl): Entire line can be converted directly so we optimise for memory;
				lines.Add(null);
				return;
			}

			LineInfo line = new(
				toUtf16.ToArray(),
				toGrapheme.ToArray()
			);

			lines.Add(line);

			toGrapheme.Clear();
			toUtf16.Clear();

			graphemeColumn = 1;
			utf16Column = 1;
		}

		foreach (TextElement element in text.EnumerateTextElements())
		{
			if (element.IsLineBreak)
			{
				MakeLine();
				continue;
			}

			foreach (char ch in element.Value)
				toGrapheme.Add(graphemeColumn);

			toUtf16.Add(utf16Column);

			utf16Column += element.Value.Length;
			graphemeColumn++;
		}

		if (toGrapheme.Any())
		{
			Debug.Assert(toUtf16.Any());
			MakeLine();
		}

		return lines.ToArray();
	}
	#endregion
}
