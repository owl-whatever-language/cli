namespace OwlDomain.ParsingTools.Positioning.Ranges;

/// <summary>
/// 	Represents a range of indexed line positions.
/// </summary>
public readonly struct IndexedPositionRange :
#if NET7_0_OR_GREATER
	IEqualityOperators<IndexedPositionRange, IndexedPositionRange, bool>,
#endif
	IEquatable<IndexedPositionRange>
{
	#region Properties
	/// <summary>The inclusive start position.</summary>
	public IndexedLinePosition Start { get; }

	/// <summary>The exclusive end position.</summary>
	/// <remarks>The exclusive end position should be on the same line as the inclusive end position.</remarks>
	public IndexedLinePosition End { get; }

	/// <summary>Whether the range spans multiple lines.</summary>
	public bool IsMultiline => Start.Line != End.Line;

	/// <summary>The amount of lines that the range spans.</summary>
	public int Lines => checked(End.Line - Start.Line + 1);

	/// <summary>The length between the start and end positions.</summary>
	public int Length => checked(End.Index - Start.Index);
	#endregion

	#region Constructors
	/// <summary>Creates a default indexed position range.</summary>
	public IndexedPositionRange()
	{
		Start = default;
		End = default;
	}

	/// <summary>Creates a new indexed position range with the given information.</summary>
	/// <param name="start">The inclusive start position.</param>
	/// <param name="end">The exclusive end position.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="end"/> position does not come after the <paramref name="start"/> position.</exception>
	public IndexedPositionRange(IndexedLinePosition start, IndexedLinePosition end)
	{
		Guard.IsGreaterThanOrEqualTo(end, start);

		Start = start;
		End = end;
	}

	/// <summary>Creates a new indexed position range with the given information.</summary>
	/// <param name="startIndex">The inclusive start index.</param>
	/// <param name="startPosition">The inclusive start position.</param>
	/// <param name="endIndex">The exclusive end index.</param>
	/// <param name="endPosition">The exclusive end position.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// 	Thrown if either:
	/// 	<list type="bullet">
	/// 		<item>Either the <paramref name="startIndex"/> or <paramref name="endIndex"/> are negative.</item>
	/// 		<item>The <paramref name="endIndex"/> comes before the <paramref name="startIndex"/>.</item>
	/// 		<item>The <paramref name="endPosition"/> comes before the <paramref name="startPosition"/>.</item>
	/// 	</list>
	/// </exception>
	public IndexedPositionRange(int startIndex, LinePosition startPosition, int endIndex, LinePosition endPosition)
	{
		Guard.IsGreaterThanOrEqualTo(startIndex, 0);
		Guard.IsGreaterThanOrEqualTo(endIndex, 0);

		Guard.IsGreaterThanOrEqualTo(endIndex, startIndex);
		Guard.IsGreaterThanOrEqualTo(endPosition, startPosition);

		Start = new(startIndex, startPosition);
		End = new(endIndex, endPosition);
	}

	/// <summary>Creates a new indexed position range with the given information.</summary>
	/// <param name="startIndex">The inclusive start index.</param>
	/// <param name="startLine">The inclusive start line.</param>
	/// <param name="startColumn">The inclusive start column.</param>
	/// <param name="endIndex">The exclusive end index.</param>
	/// <param name="endLine">The exclusive end line.</param>
	/// <param name="endColumn">The exclusive end column.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// 	Thrown if either:
	/// 	<list type="bullet">
	/// 		<item>Either the <paramref name="startIndex"/> or <paramref name="endIndex"/> are negative.</item>
	/// 		<item>Either the start/end line/column are less than or equal to zero.</item>
	/// 		<item>The <paramref name="endIndex"/> comes before the <paramref name="startIndex"/>.</item>
	/// 		<item>The <paramref name="endLine"/> comes before the <paramref name="startLine"/>.</item>
	/// 		<item>The start and end lines are the same, but the <paramref name="endColumn"/> comes before the <paramref name="startColumn"/>.</item>
	/// 	</list>
	/// </exception>
	public IndexedPositionRange(int startIndex, int startLine, int startColumn, int endIndex, int endLine, int endColumn)
	{
		Guard.IsGreaterThanOrEqualTo(startIndex, 0);
		Guard.IsGreaterThan(startLine, 0);
		Guard.IsGreaterThan(startColumn, 0);
		Guard.IsGreaterThanOrEqualTo(endIndex, 0);
		Guard.IsGreaterThan(endLine, 0);
		Guard.IsGreaterThan(endColumn, 0);

		Guard.IsGreaterThanOrEqualTo(endIndex, startIndex);
		Guard.IsGreaterThanOrEqualTo(endLine, startLine);

		if (startLine == endLine)
			Guard.IsGreaterThanOrEqualTo(endColumn, startColumn);

		Start = new(startIndex, startLine, startColumn);
		End = new(endIndex, endLine, endColumn);
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public bool Equals(IndexedPositionRange other)
	{
		return
			Start == other.Start &&
			End == other.End;
	}

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		if (obj is IndexedPositionRange other)
			return Equals(other);

		return false;
	}

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Start, End);

	/// <inheritdoc/>
	public override string ToString() => $"{Start} -> {End}";
	#endregion

	#region Operators
	/// <inheritdoc/>
	public static bool operator ==(IndexedPositionRange left, IndexedPositionRange right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(IndexedPositionRange left, IndexedPositionRange right) => left.Equals(right) is false;
	#endregion
}
