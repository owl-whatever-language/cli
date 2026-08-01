namespace OwlDomain.Owl.Config.CodeAnalysis.Parsing;

public enum ExpressionPrecedence
{
	Zero = 0,

	#region Order
	Suffix,
	#endregion

	#region Aliases
	Access = Suffix
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
	private static IReadOnlyDictionary<SyntaxKind, ExpressionPower> PropertyKeyPowers { get; } = new Dictionary<SyntaxKind, ExpressionPower>()
	{
		{ SyntaxKind.Period, ExpressionPrecedence.Access },
	};
	private static IReadOnlyDictionary<SyntaxKind, ExpressionPower> PropertyValuePowers { get; } = new Dictionary<SyntaxKind, ExpressionPower>()
	{
		{ SyntaxKind.Period, ExpressionPrecedence.Access },
	};
	#endregion

	#region Functions
	public static ExpressionPower PropertyKeyPowerOf(SyntaxKind kind) => PropertyKeyPowers.GetValueOrDefault(kind, default);
	public static ExpressionPower PropertyValuePowerOf(SyntaxKind kind) => PropertyValuePowers.GetValueOrDefault(kind, default);
	#endregion

	#region Operators
	public static implicit operator ExpressionPower(ExpressionPrecedence precedence) => new(precedence);
	#endregion
}
