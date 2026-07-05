namespace OwlDomain.ParsingTools.Syntax.Kinds;

partial struct SyntaxKind
{
	#region Trivia
	/// <summary>Represents a syntax kind for white-space trivia.</summary>
	public static SyntaxKind WhiteSpace { get; } = new("white_space", SyntaxCategory.Trivia);

	/// <summary>Represents a syntax kind for comment trivia.</summary>
	public static SyntaxKind Comment { get; } = new("comment", SyntaxCategory.Trivia);
	#endregion

	#region Punctuation
	public static SyntaxKind EqualSign { get; } = new("equal_sign");
	public static SyntaxKind DoubleEqualSign { get; } = new("double_equal_sign");
	public static SyntaxKind Semicolon { get; } = new("semicolon");
	public static SyntaxKind Comma { get; } = new("comma");
	public static SyntaxKind Colon { get; } = new("colon");
	public static SyntaxKind Period { get; } = new("period");
	public static SyntaxKind QuestionMark { get; } = new("question_mark");
	public static SyntaxKind OpenBrace { get; } = new("open_brace");
	public static SyntaxKind CloseBrace { get; } = new("close_brace");
	public static SyntaxKind OpenBracket { get; } = new("open_bracket");
	public static SyntaxKind CloseBracket { get; } = new("close_bracket");
	public static SyntaxKind OpenSquareBracket { get; } = new("open_square_bracket");
	public static SyntaxKind CloseSquareBracket { get; } = new("close_square_bracket");
	public static SyntaxKind OpenAngleBracket { get; } = new("open_angle_bracket");
	public static SyntaxKind CloseAngleBracket { get; } = new("close_angle_bracket");

	public static SyntaxKind Plus { get; } = new("plus");
	public static SyntaxKind Minus { get; } = new("minus");
	public static SyntaxKind Divide { get; } = new("divide");
	public static SyntaxKind Star { get; } = new("star");
	public static SyntaxKind Modulo { get; } = new("modulo");

	public static SyntaxKind PlusEqual { get; } = new("plus_equal");
	public static SyntaxKind MinusEqual { get; } = new("minus_equal");
	public static SyntaxKind DivideEqual { get; } = new("divide_equal");
	public static SyntaxKind StarEqual { get; } = new("star_equal");
	public static SyntaxKind ModuloEqual { get; } = new("modulo_equal");

	public static SyntaxKind NotEqual { get; } = new("not_equal");
	public static SyntaxKind LessThanOrEqual { get; } = new("less_than_or_equal");
	public static SyntaxKind GreaterThanOrEqual { get; } = new("greater_than_or_equal");
	public static SyntaxKind DoubleAmpersand { get; } = new("double_ampersand");
	public static SyntaxKind DoublePipe { get; } = new("double_pipe");
	#endregion

	#region Tokens
	public static SyntaxKind Identifier { get; } = new("identifier");
	#endregion
}
