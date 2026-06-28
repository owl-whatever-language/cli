namespace OwlDomain.ParsingTools.Syntax.Kinds;

/// <summary>
/// 	Represents a syntax kind.
/// </summary>
public readonly partial struct SyntaxKind :
#if NET7_0_OR_GREATER
	IEqualityOperators<SyntaxKind, SyntaxKind, bool>,
#endif
	IEquatable<SyntaxKind>
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string? _name;
	#endregion

	#region Properties
	/// <summary>The name of the syntax kind.</summary>
	public string Name => _name ?? "unknown";

	/// <summary>The category that the syntax kind is in.</summary>
	public SyntaxCategory Category { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a default syntax kind.</summary>
	public SyntaxKind()
	{
		_name = null;
		Category = default;
	}

	/// <summary>Creates a new syntax kind with the given information.</summary>
	/// <param name="name">The name of the syntax kind.</param>
	/// <param name="category">The category the syntax kind belongs to.</param>
	/// <exception cref="ArgumentException">Thrown if the given <paramref name="category"/> is unknown.</exception>
	public SyntaxKind(string name, SyntaxCategory category)
	{
		Guard.IsNotDefault(category);

		_name = name;
		Category = category;
	}

	/// <summary>Creates a new syntax kind with the given information.</summary>
	/// <param name="name">The name of the syntax kind.</param>
	/// <remarks>This assumes that the syntax kind refers to a token.</remarks>
	public SyntaxKind(string name) : this(name, SyntaxCategory.Token) { }
	#endregion

	#region Methods
	/// <inheritdoc/>
	public bool Equals(SyntaxKind other)
	{
		return
			Name == other.Name &&
			Category == other.Category;
	}

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		if (obj is SyntaxKind other)
			return Equals(other);

		return false;
	}

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Name, Category);

	/// <inheritdoc/>
	public override string ToString()
	{
		string category = Category.ToString();
		string name = Name;

		if (name == category)
			return name;

		return $"{name}_{category}";
	}
	#endregion

	#region Operators
	/// <inheritdoc/>
	public static bool operator ==(SyntaxKind left, SyntaxKind right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(SyntaxKind left, SyntaxKind right) => left.Equals(right) is false;
	#endregion
}

/// <summary>
/// 	Contains various guard extensions related to the <see cref="SyntaxKind"/>.
/// </summary>
public static class SyntaxKindGuardExtensions
{
	extension(Guard)
	{
		#region Functions
		/// <summary>Asserts that the given <paramref name="value"/> is of the given syntax <paramref name="category"/>.</summary>
		/// <param name="value">The value to check.</param>
		/// <param name="category">The category that the syntax kind should be a part of.</param>
		/// <param name="name">The name of the input parameter being tested.</param>
		/// <exception cref="ArgumentException">Thrown if the given <paramref name="value"/> is not of the expected syntax <paramref name="category"/>.</exception>
		public static void IsOfCategory(SyntaxKind value, SyntaxCategory category, [CallerArgumentExpression(nameof(value))] string name = "")
		{
			if (value.Category != category)
				ThrowHelper.ThrowArgumentException(name, $"The syntax kind ({value}) was not of the expected category ({category}).");
		}
		#endregion
	}
}
