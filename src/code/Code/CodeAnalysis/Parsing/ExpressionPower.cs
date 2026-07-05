namespace OwlDomain.Owl.Code.CodeAnalysis.Parsing;

public enum ExpressionPrecedence
{
	Zero = 0,

	#region Order
	Suffix,
	Equality,
	Multiplication,
	Addition,
	LogicalAnd,
	LogicalOr,
	#endregion

	#region Aliases
	Modulo = Multiplication,
	Call = Suffix,
	Assignment = Suffix,
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
	private static IReadOnlyDictionary<SyntaxKind, ExpressionPower> Powers { get; } = new Dictionary<SyntaxKind, ExpressionPower>()
	{
		{ SyntaxKind.OpenBracket, ExpressionPrecedence.Call },

		{ SyntaxKind.Divide, ExpressionPrecedence.Addition },
		{ SyntaxKind.Star, ExpressionPrecedence.Addition },
		{ SyntaxKind.Modulo, ExpressionPrecedence.Modulo },

		{ SyntaxKind.DivideEqual, ExpressionPrecedence.Assignment },
		{ SyntaxKind.StarEqual, ExpressionPrecedence.Assignment },
		{ SyntaxKind.ModuloEqual, ExpressionPrecedence.Assignment },

		{ SyntaxKind.DoubleAmpersand, ExpressionPrecedence.LogicalAnd },
		{ SyntaxKind.DoublePipe, ExpressionPrecedence.LogicalOr },

		{ SyntaxKind.Minus, ExpressionPrecedence.Addition },
		{ SyntaxKind.Plus, ExpressionPrecedence.Addition },
		{ SyntaxKind.MinusEqual, ExpressionPrecedence.Assignment },
		{ SyntaxKind.PlusEqual, ExpressionPrecedence.Assignment },

		{ SyntaxKind.EqualSign, ExpressionPrecedence.Assignment },

		{ SyntaxKind.DoubleEqualSign, ExpressionPrecedence.Equality },
		{ SyntaxKind.NotEqual, ExpressionPrecedence.Equality },

		{ SyntaxKind.LessThanOrEqual, ExpressionPrecedence.Equality },
		{ SyntaxKind.OpenAngleBracket, ExpressionPrecedence.Equality },
		{ SyntaxKind.GreaterThanOrEqual, ExpressionPrecedence.Equality },
		{ SyntaxKind.CloseAngleBracket, ExpressionPrecedence.Equality },
	};
	public int Value => (int)Precedence - (int)Associativity;
	#endregion

	#region Functions
	public static ExpressionPower PowerOf(SyntaxKind kind) => Powers.GetValueOrDefault(kind, default);
	#endregion

	#region Operators
	public static implicit operator ExpressionPower(ExpressionPrecedence precedence) => new(precedence);
	#endregion
}
