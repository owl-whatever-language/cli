namespace OwlDomain.Owl.Code.CodeAnalysis.Parsing;

public static class SyntaxKindExtensions
{
	#region Fields
	private static readonly IReadOnlyCollection<SyntaxKind> AllKeywords =
	[
		// Keywords
		SyntaxKind.Fun,
		SyntaxKind.If,
		SyntaxKind.Else,
		SyntaxKind.While,
		SyntaxKind.Return,

		// Type keywords
		SyntaxKind.Void,
	];

	private static readonly IReadOnlyCollection<SyntaxKind> BinaryOperators =
	[
		SyntaxKind.Star, SyntaxKind.Divide, SyntaxKind.Modulo,
		SyntaxKind.Plus, SyntaxKind.Minus,

		SyntaxKind.StarEqual, SyntaxKind.DivideEqual, SyntaxKind.ModuloEqual,
		SyntaxKind.PlusEqual, SyntaxKind.MinusEqual,

		SyntaxKind.DoubleEqualSign, SyntaxKind.NotEqual,
		SyntaxKind.LessThanOrEqual,  SyntaxKind.OpenAngleBracket,
		SyntaxKind.GreaterThanOrEqual,  SyntaxKind.CloseAngleBracket,

		SyntaxKind.EqualSign,

		SyntaxKind.DoubleAmpersand,
		SyntaxKind.DoublePipe,
	];
	#endregion

	extension(SyntaxKind)
	{
		#region Properties
		public static IReadOnlyCollection<SyntaxKind> AllKeywords => AllKeywords;
		public static IReadOnlyCollection<SyntaxKind> BinaryOperators => BinaryOperators;
		#endregion

		#region Keywords
		public static SyntaxKind Fun => new("fun");
		public static SyntaxKind If => new("if");
		public static SyntaxKind Else => new("else");
		public static SyntaxKind While => new("while");
		public static SyntaxKind Return => new("return");
		#endregion

		#region Type keywords
		public static SyntaxKind Void => new("void");
		#endregion

		#region String literals
		public static SyntaxKind StringStart => new("string_start");
		public static SyntaxKind InterpolatedStringStart => new("interpolated_string_start");
		public static SyntaxKind StringInterpolationStart => new("string_interpolation_start");
		public static SyntaxKind StringInterpolationEnd => new("string_interpolation_end");
		public static SyntaxKind StringEscape => new("string_escape");
		public static SyntaxKind StringText => new("string_text");
		public static SyntaxKind StringHexSequence => new("string_hex_sequence");
		public static SyntaxKind StringEnd => new("string_end");
		#endregion

		#region Number literals
		public static SyntaxKind IntegerBase => new("integer_base");
		public static SyntaxKind Integer => new("integer");
		#endregion

		#region Punctuation
		public static SyntaxKind EqualArrow => new("equal_arrow");
		#endregion
	}
}
