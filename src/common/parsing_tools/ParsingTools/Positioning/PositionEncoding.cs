namespace OwlDomain.ParsingTools.Positioning;

/// <summary>
/// 	Represents a text position encoding.
/// </summary>
public readonly struct PositionEncoding :
#if NET7_0_OR_GREATER
	IEqualityOperators<PositionEncoding, PositionEncoding, bool>,
#endif
	IEquatable<PositionEncoding>
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string? _name;
	#endregion

	#region Properties
	/// <summary>A position encoding for ASCII characters.</summary>
	public static PositionEncoding Ascii { get; } = new("ascii");

	/// <summary>A position encoding for UTF-8 characters.</summary>
	public static PositionEncoding Utf8 { get; } = new("utf-8");

	/// <summary>A position encoding for UTF-16 characters.</summary>
	public static PositionEncoding Utf16 { get; } = new("utf-16");

	/// <summary>A position encoding for UTF-32 characters.</summary>
	public static PositionEncoding Utf32 { get; } = new("utf-32");

	/// <summary>A position encoding for text elements (Otherwise also known as grapheme clusters).</summary>
	public static PositionEncoding TextElement { get; } = new("text_element");

	/// <summary>The name of the position encoding.</summary>
	public string Name => _name ?? "unknown";
	#endregion

	#region Constructors
	/// <summary>Creates a default position encoding.</summary>
	public PositionEncoding() => _name = null;

	/// <summary>Creates a new position encoding with the given <paramref name="name"/>.</summary>
	/// <param name="name">The name of the position encoding.</param>
	public PositionEncoding(string name) => _name = name;
	#endregion

	#region Methods
	/// <inheritdoc/>
	public bool Equals(PositionEncoding other) => Name == other.Name;

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		if (obj is PositionEncoding other)
			return Equals(other);

		return false;
	}

	/// <inheritdoc/>
	public override int GetHashCode() => Name.GetHashCode();

	/// <inheritdoc/>
	public override string ToString() => Name;
	#endregion


	#region Operators
	/// <inheritdoc/>
	public static bool operator ==(PositionEncoding left, PositionEncoding right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(PositionEncoding left, PositionEncoding right) => left.Equals(right) is false;
	#endregion
}
