namespace OwlDomain.ParsingTools.Positioning;

/// <summary>
/// 	Represents a combination of a line and column position, along with an index into the text.
/// </summary>
public readonly struct IndexedLinePosition :
#if NET7_0_OR_GREATER
	IEqualityOperators<IndexedLinePosition, IndexedLinePosition, bool>,
	IComparisonOperators<IndexedLinePosition, IndexedLinePosition, bool>,
#endif
	IEquatable<IndexedLinePosition>,
	IComparable<IndexedLinePosition>
{
	#region Properties
	/// <summary>The zero-based index in the text.</summary>
	public int Index { get; }

	/// <summary>The line and column position.</summary>
	public LinePosition Position { get; }

	/// <inheritdoc cref="LinePosition.Line"/>
	public int Line => Position.Line;

	/// <inheritdoc cref="LinePosition.Column"/>
	public int Column => Position.Column;
	#endregion

	#region Constructors
	/// <summary>Creates a default indexed line position.</summary>
	public IndexedLinePosition()
	{
		Index = 0;
		Position = default;
	}

	/// <summary>Creates a new indexed line position with the given information.</summary>
	/// <param name="index">The index in the text.</param>
	/// <param name="position">The line and column position.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> is less than zero.</exception>
	public IndexedLinePosition(int index, LinePosition position)
	{
		Guard.IsGreaterThanOrEqualTo(index, 0);

		Index = index;
		Position = position;
	}

	/// <summary>Creates a new indexed line position with the given information.</summary>
	/// <param name="index">The index in the text.</param>
	/// <param name="line">The one-based line number.</param>
	/// <param name="column">The one-based column number.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// 	Thrown if either the <paramref name="index"/> is less than zero.
	/// 	Or the <paramref name="line"/> or the <paramref name="column"/> is less than or equal to zero.
	/// </exception>
	public IndexedLinePosition(int index, int line, int column)
	{
		Guard.IsGreaterThanOrEqualTo(index, 0);

		Index = index;
		Position = new(line, column);
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public int CompareTo(IndexedLinePosition other) => Index.CompareTo(other.Index);

	/// <inheritdoc/>
	public bool Equals(IndexedLinePosition other)
	{
		return
			Index == other.Index &&
			Position == other.Position;
	}

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		if (obj is IndexedLinePosition other)
			return Equals(other);

		return false;
	}

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Index, Position);

	/// <inheritdoc/>
	public override string ToString() => Position.ToString();
	#endregion

	#region Operators
	/// <inheritdoc/>
	public static bool operator ==(IndexedLinePosition left, IndexedLinePosition right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(IndexedLinePosition left, IndexedLinePosition right) => left.Equals(right) is false;

	/// <inheritdoc/>
	public static bool operator <(IndexedLinePosition left, IndexedLinePosition right) => left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator >(IndexedLinePosition left, IndexedLinePosition right) => left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator <=(IndexedLinePosition left, IndexedLinePosition right) => left.CompareTo(right) <= 0;

	/// <inheritdoc/>
	public static bool operator >=(IndexedLinePosition left, IndexedLinePosition right) => left.CompareTo(right) >= 0;
	#endregion
}
