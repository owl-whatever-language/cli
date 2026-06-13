namespace OwlDomain.ParsingTools.Classification;

/// <summary>
/// 	Represents a classification kind.
/// </summary>
public readonly partial struct ClassificationKind :
#if NET7_0_OR_GREATER
	IEqualityOperators<ClassificationKind, ClassificationKind, bool>,
#endif
	IEquatable<ClassificationKind>
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string? _name;
	#endregion

	#region Properties
	/// <summary>The name of the classification kind.</summary>
	public string Name => _name ?? "unknown";
	#endregion

	#region Constructors
	/// <summary>Creates a default classification kind.</summary>
	public ClassificationKind() => _name = null;

	/// <summary>Creates a new classification kind with the given <paramref name="name"/>.</summary>
	/// <param name="name">The name for the classification kind.</param>
	public ClassificationKind(string name) => _name = name;
	#endregion

	#region Methods
	/// <inheritdoc/>
	public bool Equals(ClassificationKind other) => Name == other.Name;

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		if (obj is ClassificationKind other)
			return Equals(other);

		return false;
	}

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Name);

	/// <inheritdoc/>
	public override string ToString() => Name;

	/// <summary>Splits the current classification into its components.</summary>
	/// <returns>The classifications that make up the current classification.</returns>
	public IReadOnlyList<ClassificationKind> Split()
	{
		if (Name.IndexOf('.') < 0)
			return [this];

		return Name
			.Split('.')
			.Reverse() // Most specific one should always be checked first, so might as well reverse it here.
			.Select(s => new ClassificationKind(s))
			.ToArray();
	}
	#endregion

	#region Operators
	/// <summary>Combines the given <see langword="string"/> with the current classification.</summary>
	/// <param name="left">The current classification.</param>
	/// <param name="right">The new <see langword="string"/> addition.</param>
	/// <returns>A combination of the current classification with the new <see langword="string"/> value.</returns>
	public static ClassificationKind operator +(ClassificationKind left, string right) => new(left.Name + "." + right);

	/// <inheritdoc/>
	public static bool operator ==(ClassificationKind left, ClassificationKind right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(ClassificationKind left, ClassificationKind right) => left.Equals(right) is false;
	#endregion
}
