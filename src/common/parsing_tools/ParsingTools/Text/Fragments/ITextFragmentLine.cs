using System.CodeDom.Compiler;

namespace OwlDomain.ParsingTools.Text.Fragments;

public interface ITextFragmentLine : ITextFragmentCollection
{
	#region Properties
	int? Line { get; }
	TextFragment? Indentation { get; }
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class TextFragmentLine : TextFragmentCollection, ITextFragmentLine
{
	#region Properties
	public int? Line { get; }
	public TextFragment? Indentation
	{
		get
		{
			if (Count is 0)
				return null;

			TextFragment first = this[0];
			if (first.Classification == ClassificationKind.Indentation)
				return first;

			return null;
		}
	}
	public bool IsOnlyCommented
	{
		get
		{
			if (this.Any(f => f.IsComment))
				return this.All(f => f.IsComment || f.IsWhitespace);

			return false;
		}
	}
	#endregion

	#region Constructors
	public TextFragmentLine(int? line) => Line = line;
	public TextFragmentLine(int? line, params IEnumerable<TextFragment> collection) : base(collection) => Line = line;
	public TextFragmentLine(int? line, int capacity) : base(capacity) => Line = line;
	#endregion

	#region Functions
	public static TextFragmentLine FromParts(int? line, params IEnumerable<object?> values)
	{
		TextFragmentLine l = new(line);

		foreach (object? value in values)
		{
			if (value is null)
				continue;

			if (value is string str)
				l.Add(str, null);
			else if (value is TextFragment fragment)
				l.Add(fragment);
			else if (value is IEnumerable<TextFragment> fragments)
				l.AddRange(fragments);
			else if (value is (string text, ClassificationKind kind))
			{
				if (text is not null)
					l.Add(text, kind);
			}
			else if (value is (char ch, ClassificationKind kind2))
				l.Add(ch.ToString(), kind2);
			else if (value is IDebugNodeFactory<IDebugTreeText> factory)
				l.AddRange(factory.GetDebugNode().Fragments);
		}

		return l;
	}
	#endregion

	#region Methods
	public TextFragmentLine Replace(Func<TextFragment, TextFragment> callback)
	{
		for (int i = 0; i < Count; i++)
			this[i] = callback.Invoke(this[i]);

		return this;
	}
	public TextFragmentLine AddClassification(ClassificationKind alternate)
	{
		for (int i = 0; i < Count; i++)
			this[i] = this[i].With(alternate);

		return this;
	}
	public override string ToString() => $"{Line} | {string.Concat(this)}";
	private string DebuggerDisplay() => $"Line #{Line} | {string.Concat(this)}";
	#endregion
}

public interface ITextFragmentLineCollection : IReadOnlyList<ITextFragmentLine>, IPlainTextPrintable
{
	#region Properties
	int LowestLine { get; }
	int HighestLine { get; }
	#endregion
}

public delegate TextFragmentCollection PrefixDelegate(TextFragmentLine line);

public sealed class TextFragmentLineCollection : List<TextFragmentLine>, ITextFragmentLineCollection
{
	#region Properties
	public int LowestLine => Find(l => l.Line is not null)?.Line ?? 0;
	public int HighestLine => FindLast(l => l.Line is not null)?.Line ?? 0;
	#endregion

	#region Indexers
	ITextFragmentLine IReadOnlyList<ITextFragmentLine>.this[int index] => this[index];
	#endregion

	#region Constructors
	public TextFragmentLineCollection() { }
	public TextFragmentLineCollection(IEnumerable<TextFragmentLine> collection) : base(collection) { }
	public TextFragmentLineCollection(int capacity) : base(capacity) { }
	#endregion

	#region Index methods
	public int IndexOfLine(int lineNumber) => FindIndex(l => l.Line is not null && l.Line >= lineNumber);
	public int IndexOfLine(int lineNumber, int fallback)
	{
		int index = IndexOfLine(lineNumber);
		return index < 0 ? fallback : index;
	}
	#endregion

	#region Insert methods
	public void InsertBeforeLine(int lineNumber, TextFragmentLine line)
	{
		int index = IndexOfLine(lineNumber, 0);
		Insert(index, line);
	}
	public void InsertRangeBeforeLine(int lineNumber, IEnumerable<TextFragmentLine> lines)
	{
		int index = IndexOfLine(lineNumber, 0);
		InsertRange(index, lines);
	}
	public void InsertAfterLine(int lineNumber, TextFragmentLine line)
	{
		int index = IndexOfLine(lineNumber, 0);
		Insert(index + 1, line);
	}
	public void InsertRangeAfterLine(int lineNumber, IEnumerable<TextFragmentLine> lines)
	{
		int index = IndexOfLine(lineNumber, 0);
		InsertRange(index + 1, lines);
	}
	#endregion

	#region Prefix methods
	public TextFragmentLineCollection Prefix(PrefixDelegate callback)
	{
		foreach (TextFragmentLine line in this)
		{
			TextFragmentCollection prefix = callback.Invoke(line);
			line.InsertRange(0, prefix);
		}

		return this;
	}
	public TextFragmentLineCollection PrefixLineMargin(string marginText = "\u2502", string numberFormat = "n0")
	{
		int maxNumberWidth = HighestLine.ToString(numberFormat).Length;

		TextFragmentCollection GetPrefix(TextFragmentLine line)
		{
			string lineNumber = line.Line is null ? "" : line.Line.Value.ToString(numberFormat);
			string padding = new(' ', maxNumberWidth - lineNumber.Length);

			TextFragment number = new(lineNumber, ClassificationKind.LineNumber);
			TextFragment margin = new(marginText, ClassificationKind.Margin);
			TextFragment space = new(" ", ClassificationKind.Whitespace);

			return
			[
				new(padding, ClassificationKind.Whitespace),
				number,
				space,
				margin,
				space
			];
		}

		return Prefix(GetPrefix);
	}
	#endregion

	#region Trim methods
	public TextFragmentLineCollection TrimFirstLines()
	{
		while (true)
		{
			if (Count is 0)
				return this;

			TextFragmentLine first = this[0];
			if (first.Line is null || (first.IsWhitespace is false))
				return this;

			RemoveAt(0);
		}
	}
	public TextFragmentLineCollection TrimLastLines()
	{
		while (true)
		{
			if (Count is 0)
				return this;

			TextFragmentLine last = this[^1];
			if (last.Line is null || (last.IsWhitespace is false))
				return this;

			RemoveAt(Count - 1);
		}
	}
	public TextFragmentLineCollection TrimIndividualLines()
	{
		foreach (TextFragmentLine line in this)
			line.TrimEnd();

		return this;
	}
	public TextFragmentLineCollection TrimLines()
	{
		TrimLastLines();
		TrimFirstLines();
		TrimIndividualLines();

		return this;
	}
	public TextFragmentLineCollection TrimSharedIndent()
	{
		if (TryGetSharedIndent(out TextFragment sharedIndent) is false)
			return this;

		Debug.Assert(sharedIndent.Text.Any());

		for (int i = 0; i < Count; i++)
		{
			TextFragmentLine line = this[i];
			if (line.Any() is false)
				continue;

			TextFragment current = line[0];

			Debug.Assert(current.Text.Any());
			Debug.Assert(current.Text[0] == sharedIndent.Text[0]);

			line[0] = new(current.Text[sharedIndent.Text.Length..], ClassificationKind.Indentation, current.Syntax);
		}

		return this;
	}
	public TextFragmentLineCollection TrimCommented()
	{
		for (int i = Count - 1; i >= 0; i--)
		{
			if (this[i].IsOnlyCommented)
				RemoveAt(i);
		}

		return this;
	}
	#endregion

	#region Replace methods
	public TextFragmentLineCollection Replace(Func<TextFragment, TextFragment> callback)
	{
		foreach (TextFragmentLine line in this)
			line.Replace(callback);

		return this;
	}
	public TextFragmentLineCollection AddClassification(ClassificationKind alternate)
	{
		foreach (TextFragmentLine line in this)
			line.AddClassification(alternate);

		return this;
	}
	#endregion

	#region Methods
	public TextFragmentLineCollection AddLine(params IEnumerable<object?> values)
	{
		TextFragmentLine line = TextFragmentLine.FromParts(null, values);
		Add(line);

		return this;
	}
	public IReadOnlyList<int> GetLineNumbers()
	{
		List<int> numbers = [];

		foreach (TextFragmentLine line in this)
		{
			if (line.Line.HasValue)
				numbers.Add(line.Line.Value);
		}

		return numbers;
	}
	public bool TryGetSharedIndent(out TextFragment sharedIndent)
	{
		IEnumerable<TextFragmentLine> enumerable = this;

		if (Count is 0 || enumerable.Any(l => (l.IsWhitespace is false) && l.FirstOrDefault().Classification != ClassificationKind.Indentation))
		{
			sharedIndent = default;
			return false;
		}

		TextFragment[] indents = enumerable.Where(l => l.IsWhitespace is false).Select(l => l.First()).ToArray();
		TextFragment shortest = indents.MinBy(f => f.Text.Length);

		if (shortest.Text.Length is 0 || indents.Any(f => f.Text.FirstOrDefault() != shortest.Text[0]))
		{
			sharedIndent = default;
			return false;
		}

		sharedIndent = shortest;
		return true;
	}

	public bool TryGetLineAt(int lineNumber, [NotNullWhen(true)] out TextFragmentLine? line)
	{
		foreach (TextFragmentLine current in this)
		{
			if (current.Line == lineNumber)
			{
				line = current;
				return true;
			}
		}

		line = default;
		return false;
	}
	public TextFragmentLine? TryGetLineAt(int lineNumber) => TryGetLineAt(lineNumber, out TextFragmentLine? line) ? line : null;
	public void WritePlainText(IndentedTextWriter writer)
	{
		foreach (TextFragmentLine line in this)
		{
			line.WritePlainText(writer);
			writer.WriteLine();
		}
	}
	IEnumerator<ITextFragmentLine> IEnumerable<ITextFragmentLine>.GetEnumerator() => GetEnumerator();
	#endregion
}
