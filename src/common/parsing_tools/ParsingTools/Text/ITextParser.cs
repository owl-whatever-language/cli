namespace OwlDomain.ParsingTools.Text;

/// <summary>
/// 	Represents a parser for text elements.
/// </summary>
public interface ITextParser
{
	#region Properties
	/// <summary>The current position of the parser.</summary>
	IndexedLinePosition Position { get; }

	/// <summary>The exclusive end position for the current position.</summary>
	/// <remarks>This should be used to ensure that the end position is exclusive, but that it doesn't accidentally go onto the next line.</remarks>
	IndexedLinePosition EndPosition { get; }

	/// <summary>The text element at the current position.</summary>
	TextElement Current { get; }

	/// <summary>The text element at the next position.</summary>
	TextElement Next { get; }

	/// <summary>Whether the parser has reached the end of the input.</summary>
	bool IsAtEnd { get; }

	/// <summary>Whether the parser has any remaining text.</summary>
	bool HasRemaining { get; }

	/// <summary>Whether the parser is at the very start of the input.</summary>
	bool IsAtStart { get; }

	/// <summary>Whether the parser is at the very start of a line.</summary>
	bool IsAtStartOfLine { get; }
	#endregion

	#region Methods
	/// <summary>Gets the text element at the given <paramref name="offset"/> from the current position.</summary>
	/// <param name="offset">The offset in terms of text elements.</param>
	/// <returns>The text element at the given offset, or <see langword="default"/> if the end of the input was reached.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if the given <paramref name="offset"/> is less than zero.</exception>
	TextElement Peek(int offset);

	/// <summary>Advances the parser to the next position.</summary>
	/// <param name="amount">The amount of text elements to advance the position by.</param>
	/// <returns><see langword="true"/> if the position was moved, <see langword="false"/> if the end was already reached.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="amount"/> is less than or equal to one.</exception>
	bool Advance(int amount = 1);
	#endregion

	#region Match methods
	/// <summary>Tries to match the given <paramref name="text"/> and advance the parser.</summary>
	/// <param name="text">The text to try and match.</param>
	/// <returns><see langword="true"/> if the given <paramref name="text"/> was matched and the parser was advanced.</returns>
	bool Match(TextElement text);

	/// <summary>Tries to match the given <paramref name="text"/> and advance the parser.</summary>
	/// <param name="text">The text to try and match.</param>
	/// <returns><see langword="true"/> if the given <paramref name="text"/> was matched and the parser was advanced.</returns>
	/// <remarks>This will only match a single text element, to match a sequence use <see cref="MatchSequence(string)"/>.</remarks>
	bool Match(string text);

	/// <summary>Tries to match the given <paramref name="text"/> and advance the parser.</summary>
	/// <param name="text">The text to try and match.</param>
	/// <returns><see langword="true"/> if the given <paramref name="text"/> was matched and the parser was advanced.</returns>
	bool Match(Rune text);

	/// <summary>Tries to match the given <paramref name="text"/> and advance the parser.</summary>
	/// <param name="text">The text to try and match.</param>
	/// <returns><see langword="true"/> if the given <paramref name="text"/> was matched and the parser was advanced.</returns>
	bool Match(char text);
	#endregion

	#region MatchSequence methods
	/// <summary>Tries to match the given <paramref name="sequence"/> and advance the parser.</summary>
	/// <param name="sequence">The sequence to try and match.</param>
	/// <returns><see langword="true"/> if the given <paramref name="sequence"/> was matched and the parser was advanced.</returns>
	/// <exception cref="ArgumentException">Thrown if the <paramref name="sequence"/> was empty.</exception>
	bool MatchSequence(params scoped ReadOnlySpan<TextElement> sequence);

	/// <summary>Tries to match the given <paramref name="sequence"/> and advance the parser.</summary>
	/// <param name="sequence">The sequence to try and match.</param>
	/// <returns><see langword="true"/> if the given <paramref name="sequence"/> was matched and the parser was advanced.</returns>
	/// <exception cref="ArgumentException">Thrown if the <paramref name="sequence"/> was empty.</exception>
	bool MatchSequence(string sequence);

	/// <summary>Tries to match the given <paramref name="sequence"/> and advance the parser.</summary>
	/// <param name="sequence">The sequence to try and match.</param>
	/// <returns><see langword="true"/> if the given <paramref name="sequence"/> was matched and the parser was advanced.</returns>
	/// <exception cref="ArgumentException">Thrown if the <paramref name="sequence"/> was empty.</exception>
	bool MatchSequence(params scoped ReadOnlySpan<Rune> sequence);

	/// <summary>Tries to match the given <paramref name="sequence"/> and advance the parser.</summary>
	/// <param name="sequence">The sequence to try and match.</param>
	/// <returns><see langword="true"/> if the given <paramref name="sequence"/> was matched and the parser was advanced.</returns>
	/// <exception cref="ArgumentException">Thrown if the <paramref name="sequence"/> was empty.</exception>
	bool MatchSequence(params scoped ReadOnlySpan<char> sequence);
	#endregion

	#region MatchAny methods
	/// <summary>Tries to match any of the given <paramref name="values"/> and advance the parser.</summary>
	/// <param name="values">The collection of values to try and match.</param>
	/// <param name="match">The matched value, or <see langword="default"/> if no value was matched.</param>
	/// <returns><see langword="true"/> if a <paramref name="match"/> could be found, <see langword="false"/> otherwise.</returns>
	bool MatchAny(ReadOnlySpan<TextElement> values, out TextElement match);

	/// <summary>Tries to match any of the given <paramref name="values"/> and advance the parser.</summary>
	/// <param name="values">The collection of values to try and match.</param>
	/// <param name="match">The matched value, or <see langword="null"/> if no value was matched.</param>
	/// <returns><see langword="true"/> if a <paramref name="match"/> could be found, <see langword="false"/> otherwise.</returns>
	bool MatchAny(ReadOnlySpan<string> values, [NotNullWhen(true)] out string? match);

	/// <summary>Tries to match any of the given <paramref name="values"/> and advance the parser.</summary>
	/// <param name="values">The collection of values to try and match.</param>
	/// <param name="match">The matched value, or <see langword="default"/> if no value was matched.</param>
	/// <returns><see langword="true"/> if a <paramref name="match"/> could be found, <see langword="false"/> otherwise.</returns>
	bool MatchAny(ReadOnlySpan<Rune> values, out Rune match);

	/// <summary>Tries to match any of the given <paramref name="values"/> and advance the parser.</summary>
	/// <param name="values">The collection of values to try and match.</param>
	/// <param name="match">The matched value, or <see langword="default"/> if no value was matched.</param>
	/// <returns><see langword="true"/> if a <paramref name="match"/> could be found, <see langword="false"/> otherwise.</returns>
	bool MatchAny(ReadOnlySpan<char> values, out char match);
	#endregion
}
