namespace OwlDomain.ParsingTools.Text;

/// <summary>
/// 	Represents the base implementation for a text parser.
/// </summary>
public abstract class BaseTextParser : ITextParser
{
	#region Properties
	/// <inheritdoc/>
	public abstract IndexedLinePosition Position { get; }

	/// <inheritdoc/>
	public IndexedLinePosition EndPosition { get; private set; } = new(1, 1, 2);

	/// <inheritdoc/>
	public TextElement Current => Peek(0);

	/// <inheritdoc/>
	public TextElement Next => Peek(1);

	/// <inheritdoc/>
	public abstract bool IsAtEnd { get; }

	/// <inheritdoc/>
	public bool HasRemaining => IsAtEnd is false;

	/// <inheritdoc/>
	public bool IsAtStart => Line is 1 && Column is 1;

	/// <inheritdoc/>
	public bool IsAtStartOfLine => Column is 1;

	/// <summary>The current line.</summary>
	protected int Line { get; private set; } = 1;

	/// <summary>The current column.</summary>
	protected int Column { get; private set; } = 1;
	#endregion

	#region Methods
	/// <inheritdoc/>
	public abstract TextElement Peek(int offset);

	/// <inheritdoc/>
	public bool Advance(int amount = 1)
	{
		Guard.IsGreaterThan(amount, 0);

		if (IsAtEnd)
			return false;

		for (int i = 0; i < amount; i++)
		{
			TextElement current = Current;

			if (current == '\r' || current == '\n' || current == "\r\n")
			{
				AdvanceByOne();

				IndexedLinePosition pos = Position;
				EndPosition = new(pos.Index + 1, pos.Line, pos.Column);

				Line++;
				Column = 1;
			}
			else
			{
				AdvanceByOne();

				IndexedLinePosition pos = Position;
				EndPosition = new(pos.Index + 1, pos.Line, pos.Column + 1);

				Column++;
			}

			if (IsAtEnd)
				return true;
		}

		return true;
	}

	/// <summary>Tries to advance the parser by one text element.</summary>
	protected abstract void AdvanceByOne();
	#endregion

	#region Match methods
	/// <inheritdoc/>
	public bool Match(TextElement text)
	{
		if (Current == text)
		{
			Advance();
			return true;
		}

		return false;
	}

	/// <inheritdoc/>
	public bool Match(string text)
	{
		if (Current == text)
		{
			Advance();
			return true;
		}

		return false;
	}

	/// <inheritdoc/>
	public bool Match(Rune text)
	{
		if (Current == text)
		{
			Advance();
			return true;
		}

		return false;
	}

	/// <inheritdoc/>
	public bool Match(char text)
	{
		if (Current == text)
		{
			Advance();
			return true;
		}

		return false;
	}
	#endregion

	#region MatchSequence methods
	/// <inheritdoc/>
	public bool MatchSequence(params scoped ReadOnlySpan<TextElement> sequence)
	{
		Guard.IsNotEmpty(sequence);

		for (int i = 0; i < sequence.Length; i++)
		{
			if (Peek(i) != sequence[i])
				return false;
		}

		Advance(sequence.Length);
		return true;
	}

	/// <inheritdoc/>
	public bool MatchSequence(string sequence)
	{
		Guard.IsNotEmpty(sequence);

		int i = 0;
		foreach (TextElement current in sequence.EnumerateTextElements())
		{
			if (Peek(i) != current)
				return false;

			i++;
		}

		Advance(i);
		return true;
	}

	/// <inheritdoc/>
	public bool MatchSequence(params scoped ReadOnlySpan<Rune> sequence)
	{
		StringBuilder builder = new();
		foreach (Rune rune in sequence)
			builder.Append(rune);

		string value = builder.ToString();
		return MatchSequence(value);
	}

	/// <inheritdoc/>
	public bool MatchSequence(params scoped ReadOnlySpan<char> sequence)
	{
		string value = new(sequence);
		return MatchSequence(value);
	}
	#endregion

	#region MatchAny methods
	/// <inheritdoc/>
	public bool MatchAny(ReadOnlySpan<TextElement> values, out TextElement match)
	{
		foreach (TextElement attempt in values)
		{
			if (Current == attempt)
			{
				match = attempt;
				Advance();

				return true;
			}
		}

		match = default;
		return false;
	}

	/// <inheritdoc/>
	public bool MatchAny(ReadOnlySpan<string> values, [NotNullWhen(true)] out string? match)
	{
		foreach (string attempt in values)
		{
			if (Current == attempt)
			{
				match = attempt;
				Advance();

				return true;
			}
		}

		match = default;
		return false;
	}

	/// <inheritdoc/>
	public bool MatchAny(ReadOnlySpan<Rune> values, out Rune match)
	{
		foreach (Rune attempt in values)
		{
			if (Current == attempt)
			{
				match = attempt;
				Advance();

				return true;
			}
		}

		match = default;
		return false;
	}

	/// <inheritdoc/>
	public bool MatchAny(ReadOnlySpan<char> values, out char match)
	{
		foreach (char attempt in values)
		{
			if (Current == attempt)
			{
				match = attempt;
				Advance();

				return true;
			}
		}

		match = default;
		return false;
	}
	#endregion
}
