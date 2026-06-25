namespace OwlDomain.ParsingTools.Classification;

partial struct ClassificationKind
{
	#region Properties
	/// <summary>Represents a classification for punctuation.</summary>
	public static ClassificationKind Punctuation { get; } = new("punctuation");

	/// <summary>Represents a classification for keywords.</summary>
	public static ClassificationKind Keyword { get; } = new("keyword");

	/// <summary>Represents a classification for errors.</summary>
	public static ClassificationKind Error { get; } = new("error");
	#endregion

	#region Trivia
	/// <summary>Represents a classification for trivia.</summary>
	public static ClassificationKind Trivia { get; } = new("trivia");

	/// <summary>Represents a classification for whitespace.</summary>
	public static ClassificationKind Whitespace { get; } = Trivia + "whitespace";

	/// <summary>Represents a classification for indentation whitespace.</summary>
	public static ClassificationKind Indentation { get; } = Whitespace + "indentation";

	/// <summary>Represents a classification for line-break whitespace.</summary>
	public static ClassificationKind LineBreak { get; } = Whitespace + "line_break";

	/// <summary>Represents a classification for comments.</summary>
	public static ClassificationKind Comment { get; } = Trivia + "comment";

	/// <summary>Represents a classification for single-line comments.</summary>
	public static ClassificationKind SinglelineComment { get; } = Comment + "single_line";

	/// <summary>Represents a classification for multi-line comments.</summary>
	public static ClassificationKind MultilineComment { get; } = Comment + "multi_line";
	#endregion

	#region Literals
	/// <summary>Represents a classification for literals.</summary>
	public static ClassificationKind Literal { get; } = new("literal");

	/// <summary>Represents a classification for numbers.</summary>
	public static ClassificationKind Number { get; } = Literal + "number";

	/// <summary>Represents a classification for strings.</summary>
	public static ClassificationKind String { get; } = Literal + "string";

	/// <summary>Represents a classification for escape sequences in strings.</summary>
	public static ClassificationKind StringEscape { get; } = String + "escape";
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

	/// <summary>Represents a classification for parameters.</summary>
	public static ClassificationKind Parameter { get; } = Variable + "parameter";
	#endregion
}
