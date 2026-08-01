namespace OwlDomain.Owl.Config.CodeAnalysis.Text;

public static class TextFragmentExtensions
{
	extension(TextFragment)
	{
		#region Punctuation
		public static TextFragment Colon => new(":", ClassificationKind.Punctuation);
		public static TextFragment Semicolon => new(";", ClassificationKind.Punctuation);
		public static TextFragment Comma => new(",", ClassificationKind.Punctuation);
		public static TextFragment ClosingBrace => new("}", ClassificationKind.Punctuation);
		#endregion
	}
}
