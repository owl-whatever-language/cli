namespace OwlDomain.Owl.Code.CodeAnalysis.Text;

public static class TextFragmentExtensions
{
	extension(TextFragment)
	{
		#region Punctuation
		public static TextFragment Colon => new(":", ClassificationKind.Punctuation);
		public static TextFragment Semicolon => new(";", ClassificationKind.Punctuation);
		public static TextFragment Comma => new(",", ClassificationKind.Punctuation);
		public static TextFragment Period => new(".", ClassificationKind.Punctuation);
		public static TextFragment OpeningBracket => new("(", ClassificationKind.Punctuation);
		public static TextFragment ClosingBracket => new(")", ClassificationKind.Punctuation);
		public static TextFragment OpeningBrace => new("{", ClassificationKind.Punctuation);
		public static TextFragment ClosingBrace => new("}", ClassificationKind.Punctuation);
		public static TextFragment OpeningAngleBracket => new("<", ClassificationKind.Punctuation);
		public static TextFragment ClosingAngleBracket => new(">", ClassificationKind.Punctuation);
		public static TextFragment NumberUnderscore => new("_", ClassificationKind.Number);
		public static TextFragment EqualSign => new("=", ClassificationKind.Punctuation);
		#endregion

		#region Types
		public static TextFragment Bool => new("bool", ClassificationKind.Type);
		#endregion
	}
}
