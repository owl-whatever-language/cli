namespace OwlDomain.ParsingTools.Text;

/// <summary>
/// 	Represents a text element parser that uses a <see langword="string"/> value as the input.
/// </summary>
public sealed class StringTextParser : BaseTextParser
{
	#region Fields
	private readonly string _input;
	private readonly int[] _indices;
	private int _index;
	#endregion

	#region Properties
	/// <inheritdoc/>
	public override IndexedLinePosition Position => new(_index, Line, Column);

	/// <inheritdoc/>
	public override bool IsAtEnd => _index >= _indices.Length;
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="StringTextParser"/> instance.</summary>
	/// <param name="input">The input to parse.</param>
	public StringTextParser(string input)
	{
		_input = input;
		_indices = StringInfo.ParseCombiningCharacters(input);
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override TextElement Peek(int offset)
	{
		Guard.IsGreaterThanOrEqualTo(offset, 0);

		if (IsAtEnd)
			return default;

		int lookupIndex = _index + offset;
		if (lookupIndex >= _indices.Length)
			return default;

		int index = _indices[lookupIndex];
		string str = StringInfo.GetNextTextElement(_input, index);

		return new(str);
	}

	/// <inheritdoc/>
	protected override void AdvanceByOne() => _index++;
	#endregion
}
