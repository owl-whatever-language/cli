namespace OwlDomain.ParsingTools.Positioning.Ranges;

/// <summary>
/// 	Represents a range of line positions.
/// </summary>
public readonly struct PositionRange :
#if NET7_0_OR_GREATER
	IEqualityOperators<PositionRange, PositionRange, bool>,
#endif
	IEquatable<PositionRange>
{
	#region Properties
	/// <summary>The inclusive start position.</summary>
	public LinePosition Start { get; }

	/// <summary>The exclusive end position.</summary>
	/// <remarks>The exclusive end position should be on the same line as the inclusive end position.</remarks>
	public LinePosition End { get; }

	/// <summary>Whether the range spans multiple lines.</summary>
	public bool IsMultiline => Start.Line != End.Line;

	/// <summary>The amount of lines that the range spans.</summary>
	public int Lines => checked(End.Line - Start.Line + 1);
	#endregion

	#region Constructors
	/// <summary>Creates a default position range.</summary>
	public PositionRange()
	{
		Start = default;
		End = default;
	}

	/// <summary>Creates a new position range with the given information.</summary>
	/// <param name="start">The inclusive start position.</param>
	/// <param name="end">The exclusive end position.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="end"/> position does not come after the <paramref name="start"/> position.</exception>
	public PositionRange(LinePosition start, LinePosition end)
	{
		Guard.IsGreaterThanOrEqualTo(end, start);

		Start = start;
		End = end;
	}

	/// <summary>Creates a new position range with the given information.</summary>
	/// <param name="startLine">The inclusive start line.</param>
	/// <param name="startColumn">The inclusive start column.</param>
	/// <param name="endLine">The exclusive end line.</param>
	/// <param name="endColumn">The exclusive end column.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// 	Throw if either of the arguments is less than or equal to zero.
	/// 	Or if the start and end lines are the same, but the <paramref name="endColumn"/> comes before the <paramref name="startColumn"/>.
	/// </exception>
	public PositionRange(int startLine, int startColumn, int endLine, int endColumn)
	{
		Guard.IsGreaterThan(startLine, 0);
		Guard.IsGreaterThan(startColumn, 0);
		Guard.IsGreaterThan(endLine, 0);
		Guard.IsGreaterThan(endColumn, 0);
		Guard.IsGreaterThanOrEqualTo(endLine, startLine);

		if (startLine == endLine)
			Guard.IsGreaterThanOrEqualTo(endColumn, startColumn);

		Start = new(startLine, startColumn);
		End = new(endLine, endColumn);
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public bool Equals(PositionRange other)
	{
		return
			Start == other.Start &&
			End == other.End;
	}

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		if (obj is PositionRange other)
			return Equals(other);

		return false;
	}

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Start, End);

	/// <inheritdoc/>
	public override string ToString() => $"{Start} -> {End}";

	public bool Contains(LinePosition position) => position >= Start && position <= End;
	#endregion

	#region Operators
	/// <inheritdoc/>
	public static bool operator ==(PositionRange left, PositionRange right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(PositionRange left, PositionRange right) => left.Equals(right) is false;
	#endregion
}
