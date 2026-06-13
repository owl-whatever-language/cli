namespace OwlDomain.ParsingTools.Classification;

partial struct ClassificationKind
{
	#region Properties
	/// <summary>Represents a classification for punctuation.</summary>
	public static ClassificationKind Punctuation { get; } = new("punctuation");

	/// <summary>Represents a classification for keywords.</summary>
	public static ClassificationKind Keyword { get; } = new("keyword");
	#endregion

	#region Literals
	/// <summary>Represents a classification for literals.</summary>
	public static ClassificationKind Literal { get; } = new("literal");

	/// <summary>Represents a classification for numbers.</summary>
	public static ClassificationKind Number { get; } = Literal + "number";

	/// <summary>Represents a classification for strings.</summary>
	public static ClassificationKind String { get; } = Literal + "string";
	#endregion

	#region Identifiers
	/// <summary>Represents a classification for identifiers.</summary>
	public static ClassificationKind Identifier { get; } = new("identifier");

	/// <summary>Represents a classification for types.</summary>
	public static ClassificationKind Type { get; } = Identifier + "type";

	/// <summary>Represents a classification for functions.</summary>
	public static ClassificationKind Function { get; } = Identifier + "function";

	/// <summary>Represents a classification for variables.</summary>
	public static ClassificationKind Variable { get; } = Identifier + "variable";
	#endregion
}
