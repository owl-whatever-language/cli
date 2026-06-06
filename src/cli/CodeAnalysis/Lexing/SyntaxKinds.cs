namespace OwlDomain.Owl.CLI.CodeAnalysis.Lexing;

public static class TokenTypes
{
	extension(SyntaxKind)
	{
		#region Properties
		public static SyntaxKind Semicolon => new("semicolon", SyntaxCategory.Token);
		public static SyntaxKind OpenBracket => new("open_bracket", SyntaxCategory.Token);
		public static SyntaxKind CloseBracket => new("close_bracket", SyntaxCategory.Token);
		public static SyntaxKind EqualSign => new("equal_sign", SyntaxCategory.Token);
		public static SyntaxKind StringLiteral => new("string_literal", SyntaxCategory.Token);
		public static SyntaxKind Identifier => new("identifier", SyntaxCategory.Token);
		#endregion
	}
}
