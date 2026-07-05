namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics;

public enum OperatorKind
{
	LessThan,
	LessThanOrEqual,
	GreaterThan,
	GreaterThanOrEqual,

	Equal,
	NotEqual,

	Add,
	Subtract,
	Multiply,
	Divide,
	Modulo,

	LogicalAnd,
	LogicalOr,
}

public static class OperatorKindExtensions
{
	#region Fields
	private static readonly Dictionary<SyntaxKind, OperatorKind> Operators = new()
	{
		{ SyntaxKind.OpenAngleBracket, OperatorKind.LessThan },
		{ SyntaxKind.LessThanOrEqual, OperatorKind.LessThanOrEqual },
		{ SyntaxKind.CloseAngleBracket, OperatorKind.GreaterThan },
		{ SyntaxKind.GreaterThanOrEqual, OperatorKind.GreaterThanOrEqual },

		{ SyntaxKind.DoubleEqualSign, OperatorKind.Equal },
		{ SyntaxKind.NotEqual, OperatorKind.NotEqual },

		{ SyntaxKind.Plus, OperatorKind.Add },
		{ SyntaxKind.Minus, OperatorKind.Subtract },
		{ SyntaxKind.Star, OperatorKind.Multiply },
		{ SyntaxKind.Divide, OperatorKind.Divide },
		{ SyntaxKind.Modulo, OperatorKind.Modulo },

		{ SyntaxKind.PlusEqual, OperatorKind.Add },
		{ SyntaxKind.MinusEqual, OperatorKind.Subtract },
		{ SyntaxKind.StarEqual, OperatorKind.Multiply },
		{ SyntaxKind.DivideEqual, OperatorKind.Divide },
		{ SyntaxKind.ModuloEqual, OperatorKind.Modulo },

		{ SyntaxKind.DoubleAmpersand, OperatorKind.LogicalAnd },
		{ SyntaxKind.DoublePipe, OperatorKind.LogicalOr },
	};
	private static readonly HashSet<SyntaxKind> CompoundAssignments =
	[
		SyntaxKind.PlusEqual, SyntaxKind.MinusEqual,
		SyntaxKind.StarEqual, SyntaxKind.DivideEqual, SyntaxKind.ModuloEqual
	];
	#endregion

	extension(SyntaxKind kind)
	{
		#region Methods
		public OperatorKind GetOperator()
		{
			if (Operators.TryGetValue(kind, out OperatorKind op))
				return op;

			ThrowHelper.ThrowInvalidOperationException($"Unhandled operator kind {kind}.");
			return default;
		}
		public bool IsCompoundAssignmentOperator() => CompoundAssignments.Contains(kind);
		#endregion
	}
}
