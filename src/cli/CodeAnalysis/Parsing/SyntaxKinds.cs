namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing;

public static class TokenTypes
{
	extension(SyntaxKind)
	{
		#region Properties
		public static SyntaxKind Invocation => new("invocation", SyntaxCategory.Expression);
		public static SyntaxKind Literal => new("literal", SyntaxCategory.Expression);
		public static SyntaxKind Access => new("access", SyntaxCategory.Expression);
		public static SyntaxKind ExpressionStatement => new("expression", SyntaxCategory.Statement);
		public static SyntaxKind VariableDeclaration => new("variable_declaration", SyntaxCategory.Statement);
		#endregion
	}
}
