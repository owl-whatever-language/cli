namespace OwlDomain.ParsingTools.Classification;

partial struct ClassificationKind
{
	#region Properties
	/// <summary>Represents a classification for punctuation.</summary>
	public static ClassificationKind Punctuation { get; } = new("punctuation");

	/// <summary>Represents a classification for keywords.</summary>
	public static ClassificationKind Keyword { get; } = new("keyword");

	/// <summary>Represents a classification for identifiers.</summary>
	public static ClassificationKind Identifier { get; } = new("identifier");

	/// <summary>Represents a classification for types.</summary>
	public static ClassificationKind Type { get; } = new("type");

	/// <summary>Represents a classification for functions.</summary>
	public static ClassificationKind Function { get; } = new("function");

	/// <summary>Represents a classification for variables.</summary>
	public static ClassificationKind Variable { get; } = new("variable");

	/// <summary>Represents a classification for literals.</summary>
	public static ClassificationKind Literal { get; } = new("literal");

	/// <summary>Represents a classification for numbers.</summary>
	public static ClassificationKind Number { get; } = new("number");

	/// <summary>Represents a classification for strings.</summary>
	public static ClassificationKind String { get; } = new("string");
	#endregion
}
