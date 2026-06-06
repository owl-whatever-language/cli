namespace OwlDomain.ParsingTools.Text;

/// <summary>
/// 	Represents a single element in a piece of text. Also know as a grapheme cluster.
/// </summary>
public readonly struct TextElement :
#if NET7_0_OR_GREATER
	IEqualityOperators<TextElement, TextElement, bool>,
	IEqualityOperators<TextElement, string?, bool>,
	IEqualityOperators<TextElement, Rune, bool>,
	IEqualityOperators<TextElement, char, bool>,
#endif
	IEquatable<TextElement>,
	IEquatable<string?>,
	IEquatable<Rune>,
	IEquatable<char>
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string? _value;
	#endregion

	#region Properties
	/// <summary>The value of the text element.</summary>
	public string Value => _value ?? "\0";

	/// <summary>The value of the text element as a <see cref="char"/> value.</summary>
	/// <remarks>This will return <c>\0</c> if the <see cref="Value"/> contains more than just a single <see langword="char"/> value.</remarks>
	public char AsChar => Value.Length is 1 ? Value[0] : '\0';

	/// <summary>The value of the text element as a <see cref="Rune"/> value.</summary>
	/// <remarks>This will return <c>\0</c> if the <see cref="Value"/> contains more than just a single <see cref="Rune"/> value.</remarks>
	public Rune AsRune
	{
		get
		{
			Rune rune = Rune.GetRuneAt(Value, 0);
			if (rune.Utf16SequenceLength == Value.Length)
				return rune;

			return new('\0');
		}
	}
	#endregion

	#region Constructors
	/// <summary>Creates a default text element.</summary>
	public TextElement() => _value = null;

	/// <summary>Creates a new text element with the given <paramref name="value"/>.</summary>
	/// <param name="value">The value of the text element (grapheme cluster).</param>
	/// <exception cref="ArgumentException">Thrown if the given <paramref name="value"/> doesn't contain exactly one text element.</exception>
	public TextElement(string value)
	{
		if (value.Length is 0 || StringInfo.GetNextTextElementLength(value) != value.Length)
			ThrowHelper.ThrowArgumentException(nameof(value), "The value was supposed to contain exactly one text element (grapheme cluster).");

		_value = value;
	}

	/// <summary>Creates a new text element with the given <paramref name="value"/>.</summary>
	/// <param name="value">The value of the text element (grapheme cluster).</param>
	public TextElement(Rune value)
	{
		_value = value.ToString();
	}

	/// <summary>Creates a new text element with the given <paramref name="value"/>.</summary>
	/// <param name="value">The value of the text element (grapheme cluster).</param>
	public TextElement(char value)
	{
		_value = value.ToString();
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public bool Equals(TextElement other) => Value == other.Value;

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] string? other) => Value == other;

	/// <inheritdoc/>
	public bool Equals(Rune other)
	{
		if (Value.Length != other.Utf16SequenceLength)
			return false;

		Rune rune = Rune.GetRuneAt(Value, 0);
		return rune == other;
	}
	/// <inheritdoc/>
	public bool Equals(char other) => Value.Length is 1 && Value[0] == other;

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		return obj switch
		{
			TextElement other => Equals(other),
			string other => Equals(other),
			Rune other => Equals(other),
			char other => Equals(other),

			_ => false,
		};
	}

	/// <inheritdoc/>
	public override int GetHashCode() => Value.GetHashCode();

	/// <inheritdoc/>
	public override string ToString() => Value;
	#endregion

	#region Operators
	/// <inheritdoc/>
	public static bool operator ==(TextElement left, TextElement right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator ==(TextElement left, [NotNullWhen(true)] string? right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator ==(TextElement left, Rune right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator ==(TextElement left, char right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(TextElement left, TextElement right) => left.Equals(right) is false;

	/// <inheritdoc/>
	public static bool operator !=(TextElement left, [NotNullWhen(false)] string? right) => left.Equals(right) is false;

	/// <inheritdoc/>
	public static bool operator !=(TextElement left, Rune right) => left.Equals(right) is false;

	/// <inheritdoc/>
	public static bool operator !=(TextElement left, char right) => left.Equals(right) is false;
	#endregion
}

/// <summary>
/// 	Contains various extensions related to the <see cref="TextElement"/> type.
/// </summary>
public static class TextElementExtensions
{
	extension(string value)
	{
		#region Methods
		/// <summary>Enumerates through all of the text elements in the <see langword="string"/>.</summary>
		/// <returns>An enumerable of the text elements in the value.</returns>
		public IEnumerable<TextElement> EnumerateTextElements()
		{
			TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(value);
			while (enumerator.MoveNext())
			{
				string element = enumerator.GetTextElement();
				yield return new(element);
			}
		}
		#endregion
	}

	extension(TextElement element)
	{
		#region Methods
		/// <summary>Whether the text element is a white-space.</summary>
		/// <remarks>This will not treat line breaks as white-space.</remarks>
		public bool IsWhiteSpace => element.IsLineBreak is false && element.Value.IsWhiteSpace();

		/// <summary>Whether the line break is considered a line break.</summary>
		/// <remarks>A line break is one of: <c>\r</c>, <c>\n</c> or <c>\r\n</c>.</remarks>
		public bool IsLineBreak => element.Value == "\r" || element.Value == "\n" || element.Value == "\r\n";
		#endregion
	}

	extension(StringBuilder builder)
	{
		#region Methods
		/// <summary>Appends the given text <paramref name="element"/> to the <see langword="string"/> builder.</summary>
		/// <param name="element">The text element to append.</param>
		/// <returns>A reference to the used <see cref="StringBuilder"/>.</returns>
		public StringBuilder Append(TextElement element) => builder.Append(element.Value);
		#endregion
	}
}
