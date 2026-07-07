namespace OwlDomain.Owl.Code.CodeAnalysis;

public static class BranchingExpressionExtensions
{
	#region Functions
	private static bool WillBranch(IConcreteExpressionSyntax expression)
	{
		return expression switch
		{
			IConcreteGetExpressionSyntax => false,
			IConcreteBinaryExpressionSyntax binary => WillBranch(binary),
			// Note(Nightowl): Assignments still require consideration here;

			_ => WillBranchGeneral(expression)
		};
	}
	private static bool WillBranchGeneral(IConcreteExpressionSyntax expression)
	{
		return expression.Search<IConcreteExpressionSyntax>(WillBranch, includeSelf: false) is not null;
	}
	private static bool WillBranch(IConcreteBinaryExpressionSyntax binary)
	{
		if (binary.IsBranchingOperator)
			return true;

		return WillBranch(binary.Left) || WillBranch(binary.Right);
	}
	#endregion

	extension(IConcreteExpressionSyntax expression)
	{
		#region Properties
		public bool WillBranch => WillBranch(expression);
		#endregion
	}
	extension(IConcreteBinaryExpressionSyntax expression)
	{
		#region Properties
		public bool IsBranchingOperator
		{
			get
			{
				SyntaxKind op = expression.Operator.Kind;
				return
					op == SyntaxKind.DoubleAmpersand ||
					op == SyntaxKind.DoublePipe
				;
			}
		}
		#endregion
	}
}
