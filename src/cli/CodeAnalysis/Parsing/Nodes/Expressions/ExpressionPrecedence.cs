namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing.Nodes.Expressions;

public enum ExpressionPrecedence
{
	Zero = 0,

	#region Order
	Suffix,
	#endregion

	#region Aliases
	Call = Suffix,
	#endregion
}

public enum ExpressionAssociativity
{
	Left = 0,
	Right = 1
}

public readonly record struct ExpressionPower(ExpressionPrecedence Precedence, ExpressionAssociativity Associativity = ExpressionAssociativity.Left)
{
	#region Properties
	public int Value => (int)Precedence - (int)Associativity;
	#endregion

	#region Functions
	public static ExpressionPower PowerOf(SyntaxKind kind)
	{
		if (kind == SyntaxKind.OpenBracket)
			return new(ExpressionPrecedence.Call);

		return default;
	}
	#endregion
}
