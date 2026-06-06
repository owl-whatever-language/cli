namespace OwlDomain.ParsingTools.Diagnostics;

/// <summary>
/// 	Represents a diagnostic kind.
/// </summary>
public readonly struct DiagnosticKind :
#if NET7_0_OR_GREATER
	IEqualityOperators<DiagnosticKind, DiagnosticKind, bool>,
#endif
	IEquatable<DiagnosticKind>
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string? _name;
	#endregion

	#region Properties
	/// <summary>A diagnostic kind that represents an error.</summary>
	public static DiagnosticKind Error { get; } = new("error");

	/// <summary>A diagnostic kind that represents a warning.</summary>
	public static DiagnosticKind Warning { get; } = new("warning");

	/// <summary>A diagnostic kind that represents a suggestion.</summary>
	public static DiagnosticKind Suggestion { get; } = new("suggestion");

	/// <summary>The name of the diagnostic kind.</summary>
	public string Name => _name ?? "unknown";
	#endregion

	#region Constructors
	/// <summary>Creates a default diagnostic kind.</summary>
	public DiagnosticKind() => _name = null;

	/// <summary>Creates a new diagnostic kind with the given <paramref name="name"/>.</summary>
	/// <param name="name">The name for the diagnostic kind.</param>
	public DiagnosticKind(string name) => _name = name;
	#endregion

	#region Methods
	/// <inheritdoc/>
	public bool Equals(DiagnosticKind other) => Name == other.Name;

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		if (obj is DiagnosticKind other)
			return Equals(other);

		return false;
	}

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Name);

	/// <inheritdoc/>
	public override string ToString() => Name;
	#endregion

	#region Operators
	/// <inheritdoc/>
	public static bool operator ==(DiagnosticKind left, DiagnosticKind right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(DiagnosticKind left, DiagnosticKind right) => left.Equals(right) is false;
	#endregion
}
