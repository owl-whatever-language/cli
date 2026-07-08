namespace OwlDomain.ParsingTools.Classification;

partial struct ClassificationKind
{
	#region Properties
	/// <summary>Represents a classification for punctuation.</summary>
	public static ClassificationKind Punctuation { get; } = new("punctuation");

	/// <summary>Represents a classification for punctuation operators.</summary>
	public static ClassificationKind Operator { get; } = Punctuation + "operator";

	/// <summary>Represents a classification for keywords.</summary>
	public static ClassificationKind Keyword { get; } = new("keyword");
	#endregion

	#region Diagnostics
	/// <summary>Represents a classification for diagnostic messages.</summary>
	public static ClassificationKind Diagnostic { get; } = new("diagnostic");

	/// <summary>Represents a classification for errors.</summary>
	public static ClassificationKind Error { get; } = Diagnostic + "error";

	/// <summary>Represents a classification for warnings.</summary>
	public static ClassificationKind Warning { get; } = Diagnostic + "warning";

	/// <summary>Represents a classification for suggestions.</summary>
	public static ClassificationKind Suggestion { get; } = Diagnostic + "suggestion";

	/// <summary>Represents a classification for hints.</summary>
	public static ClassificationKind Hint { get; } = Diagnostic + "hint";
	#endregion

	#region Source printing
	public static ClassificationKind PrettySource { get; } = new("pretty_source");
	public static ClassificationKind LineNumber { get; } = PrettySource + "line_number";
	public static ClassificationKind Margin { get; } = PrettySource + "margin";
	public static ClassificationKind Message { get; } = PrettySource + "message";
	public static ClassificationKind Dim { get; } = PrettySource + "dim";
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

	/// <summary>Represents a classification for booleans.</summary>
	public static ClassificationKind Boolean { get; } = Literal + "boolean";
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

	/// <summary>Represents a classification for file names.</summary>
	public static ClassificationKind File { get; } = Identifier + "file";
	#endregion
}
