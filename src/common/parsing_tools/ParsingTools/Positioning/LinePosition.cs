namespace OwlDomain.ParsingTools.Positioning;

/// <summary>
/// 	Represents a line position.
/// </summary>
[Serializable]
public readonly struct LinePosition :
#if NET7_0_OR_GREATER
	IEqualityOperators<LinePosition, LinePosition, bool>,
	IComparisonOperators<LinePosition, LinePosition, bool>,
#endif
	IEquatable<LinePosition>,
	IComparable<LinePosition>
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly int _line, _column;
	#endregion

	#region Properties
	/// <summary>The one-based line number.</summary>
	public int Line => _line + 1;

	/// <summary>The one-based column number.</summary>
	public int Column => _column + 1;
	#endregion

	#region Constructors
	/// <summary>Creates a default line position.</summary>
	public LinePosition()
	{
		_line = 0;
		_column = 0;
	}

	/// <summary>Creates a new line position with the given information.</summary>
	/// <param name="line">The one-based line number.</param>
	/// <param name="column">The one-based column number.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// 	Thrown if either the <paramref name="line"/> or the
	/// 	<paramref name="column"/> is less than or equal to zero.
	/// </exception>
	public LinePosition(int line, int column)
	{
		Guard.IsGreaterThan(line, 0);
		Guard.IsGreaterThan(column, 0);

		_line = line - 1;
		_column = column - 1;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public int CompareTo(LinePosition other)
	{
		int line = _line.CompareTo(other._line);
		if (line is not 0)
			return line;

		return _column.CompareTo(other._column);
	}

	/// <inheritdoc/>
	public bool Equals(LinePosition other)
	{
		return
			_line == other._line &&
			_column == other._column;
	}

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		if (obj is LinePosition other)
			return Equals(other);

		return false;
	}

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(_line, _column);

	/// <inheritdoc/>
	public override string ToString() => $"{Line}, {Column}";
	#endregion

	#region Operators
	/// <inheritdoc/>
	public static bool operator ==(LinePosition left, LinePosition right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(LinePosition left, LinePosition right) => left.Equals(right) is false;

	/// <inheritdoc/>
	public static bool operator <(LinePosition left, LinePosition right) => left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator >(LinePosition left, LinePosition right) => left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator <=(LinePosition left, LinePosition right) => left.CompareTo(right) <= 0;

	/// <inheritdoc/>
	public static bool operator >=(LinePosition left, LinePosition right) => left.CompareTo(right) >= 0;
	#endregion
}
