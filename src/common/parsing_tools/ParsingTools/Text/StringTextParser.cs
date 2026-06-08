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
	private string? _cache0, _cache1;
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

		if (offset is 0)
			return new(_cache0 ??= PeekNoGuard(lookupIndex), false);

		if (offset is 1)
			return new(_cache1 ??= PeekNoGuard(lookupIndex), false);

		string value = PeekNoGuard(offset);
		return new(value, false);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private string PeekNoGuard(int lookupIndex)
	{
		int index = _indices[lookupIndex];
		int nextIndex = lookupIndex + 1 == _indices.Length ? _input.Length : _indices[lookupIndex + 1];
		string str = _input[index..nextIndex];

		return str;
	}

	/// <inheritdoc/>
	protected override void AdvanceByOne()
	{
		_cache0 = _cache1;
		_cache1 = null;
		_index++;
	}
	#endregion
}
