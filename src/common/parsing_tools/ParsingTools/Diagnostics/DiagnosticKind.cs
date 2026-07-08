namespace OwlDomain.ParsingTools.Diagnostics;

/// <summary>
/// 	Represents a diagnostic kind.
/// </summary>
public readonly struct DiagnosticKind :
#if NET7_0_OR_GREATER
	IEqualityOperators<DiagnosticKind, DiagnosticKind, bool>,
	IComparisonOperators<DiagnosticKind, DiagnosticKind, bool>,
#endif
	IEquatable<DiagnosticKind>,
	IComparable<DiagnosticKind>
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string? _name;
	#endregion

	#region Properties
	/// <summary>A diagnostic kind that represents an error.</summary>
	/// <remarks>This diagnostic kind has a level of <c>90</c>.</remarks>
	public static DiagnosticKind Error { get; } = new("error", 90);

	/// <summary>A diagnostic kind that represents a warning.</summary>
	/// <remarks>This diagnostic kind has a level of <c>60</c>.</remarks>
	public static DiagnosticKind Warning { get; } = new("warning", 60);

	/// <summary>A diagnostic kind that represents a suggestion.</summary>
	/// <remarks>This diagnostic kind has a level of <c>30</c>.</remarks>
	public static DiagnosticKind Suggestion { get; } = new("suggestion", 30);

	/// <summary>The name of the diagnostic kind.</summary>
	public string Name => _name ?? "unknown";

	/// <summary>The level of the diagnostic.</summary>
	/// <remarks>Higher value means more the diagnostic is more severe. A value of <c>0</c> means no particular severity.</remarks>
	public int Level { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a default diagnostic kind.</summary>
	public DiagnosticKind() => _name = null;

	/// <summary>Creates a new diagnostic kind with the given <paramref name="name"/>.</summary>
	/// <param name="name">The name for the diagnostic kind.</param>
	/// <param name="level">The level of the diagnostic.</param>
	public DiagnosticKind(string name, int level = 0)
	{
		_name = name;
		Level = level;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public int CompareTo(DiagnosticKind other) => Level.CompareTo(other.Level);

	/// <inheritdoc/>
	public bool Equals(DiagnosticKind other) => Name == other.Name && Level == other.Level;

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		if (obj is DiagnosticKind other)
			return Equals(other);

		return false;
	}

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Name, Level);

	/// <inheritdoc/>
	public override string ToString() => Name;
	#endregion

	#region Operators
	/// <inheritdoc/>
	public static bool operator ==(DiagnosticKind left, DiagnosticKind right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(DiagnosticKind left, DiagnosticKind right) => left.Equals(right) is false;

	/// <inheritdoc/>
	public static bool operator <(DiagnosticKind left, DiagnosticKind right) => left.Level < right.Level;

	/// <inheritdoc/>
	public static bool operator >(DiagnosticKind left, DiagnosticKind right) => left.Level > right.Level;

	/// <inheritdoc/>
	public static bool operator <=(DiagnosticKind left, DiagnosticKind right) => left.Level <= right.Level;

	/// <inheritdoc/>
	public static bool operator >=(DiagnosticKind left, DiagnosticKind right) => left.Level >= right.Level;
	#endregion
}

public static class DiagnosticKindExtensions
{
	extension(DiagnosticKind kind)
	{
		#region Methods
		public ClassificationKind ToClassification()
		{
			if (kind >= DiagnosticKind.Error)
				return ClassificationKind.Error;

			if (kind >= DiagnosticKind.Warning)
				return ClassificationKind.Warning;

			if (kind >= DiagnosticKind.Suggestion)
				return ClassificationKind.Suggestion;

			return ClassificationKind.Hint;
		}
		#endregion
	}
}
