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

		_cache0 = PeekNoCache(0);
		_cache1 = PeekNoCache(1);
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override TextElement Peek(int offset)
	{
		Guard.IsGreaterThanOrEqualTo(offset, 0);

		if (offset is 0)
		{
			_cache0 ??= PeekNoCache(0);
			return _cache0 is null ? default : new(_cache0);
		}

		if (offset is 1)
		{
			_cache1 ??= PeekNoCache(1);
			return _cache1 is null ? default : new(_cache1);
		}

		string? value = PeekNoCache(offset);
		return value is null ? default : new(value);
	}
	private string? PeekNoCache(int offset)
	{
		Guard.IsGreaterThanOrEqualTo(offset, 0);

		if (IsAtEnd)
			return null;

		int lookupIndex = _index + offset;
		if (lookupIndex >= _indices.Length)
			return null;

		return PeekNoGuard(lookupIndex);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private string PeekNoGuard(int lookupIndex)
	{
		int index = _indices[lookupIndex];
		string str = StringInfo.GetNextTextElement(_input, index);

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
