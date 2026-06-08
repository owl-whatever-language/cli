namespace OwlDomain.ParsingTools.Syntax;

/// <summary>
/// 	Represents a syntax category.
/// </summary>
public readonly struct SyntaxCategory :
#if NET7_0_OR_GREATER
	IEqualityOperators<SyntaxCategory, SyntaxCategory, bool>,
#endif
	IEquatable<SyntaxCategory>
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string? _name;
	#endregion

	#region Properties
	/// <summary>A syntax category for tokens.</summary>
	public static SyntaxCategory Token { get; } = new("token");

	/// <summary>A syntax category for trivia nodes.</summary>
	public static SyntaxCategory Trivia { get; } = new("trivia");

	/// <summary>A syntax category for statement nodes.</summary>
	public static SyntaxCategory Statement { get; } = new("statement");

	/// <summary>A syntax category for expression nodes.</summary>
	public static SyntaxCategory Expression { get; } = new("expression");

	/// <summary>A syntax category for declaration nodes.</summary>
	public static SyntaxCategory Declaration { get; } = new("declaration");

	/// <summary>A syntax category for root document nodes.</summary>
	public static SyntaxCategory Document { get; } = new("document");

	/// <summary>The name of the syntax category.</summary>
	public string Name => _name ?? "unknown";
	#endregion

	#region Constructors
	/// <summary>Creates a default syntax category.</summary>
	public SyntaxCategory() => _name = null;

	/// <summary>Creates a new syntax category with the given <paramref name="name"/>.</summary>
	/// <param name="name">The name of the syntax category.</param>
	public SyntaxCategory(string name) => _name = name;
	#endregion

	#region Methods
	/// <inheritdoc/>
	public bool Equals(SyntaxCategory other) => Name == other.Name;

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		if (obj is SyntaxCategory other)
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
	public static bool operator ==(SyntaxCategory left, SyntaxCategory right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(SyntaxCategory left, SyntaxCategory right) => left.Equals(right) is false;
	#endregion
}

/// <summary>
/// 	Contains various extensions related to the <see cref="SyntaxCategory"/> type.
/// </summary>
public static class SyntaxCategoryExtensions
{
	extension(SyntaxCategory category)
	{
		#region Properties
		/// <summary>Whether the syntax category represents a token.</summary>
		public bool IsToken => category == SyntaxCategory.Token;

		/// <summary>Whether the syntax category represents a trivia node.</summary>
		public bool IsTrivia => category == SyntaxCategory.Trivia;

		/// <summary>Whether the syntax category represents a statement node.</summary>
		public bool IsStatement => category == SyntaxCategory.Statement;

		/// <summary>Whether the syntax category represents an expression node.</summary>
		public bool IsExpression => category == SyntaxCategory.Expression;

		/// <summary>Whether the syntax category represents a declaration node.</summary>
		public bool IsDeclaration => category == SyntaxCategory.Declaration;
		#endregion
	}
}
