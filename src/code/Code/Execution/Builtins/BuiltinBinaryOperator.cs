namespace OwlDomain.Owl.Code.Execution.Builtins;

internal class BuiltinBinaryOperator : IBinaryOperator
{
	#region Properties
	public OperatorKind Kind { get; }
	public IType Left { get; }
	public IType Right { get; }
	public IType Result { get; }
	public BuiltinFunction AsFunction { get; }
	IFunction IBinaryOperator.AsFunction => AsFunction;
	#endregion

	#region Constructors
	public BuiltinBinaryOperator(OperatorKind kind, IType left, IType right, IType result, BuiltinFunction function)
	{
		Kind = kind;
		Left = left;
		Right = right;
		Result = result;
		AsFunction = function;
	}
	#endregion

	#region Methods
	public TextFragmentCollection GetDebugText()
	{
		TextFragmentCollection fragments = [];

		fragments.Add(Kind.Operator, ClassificationKind.Punctuation);
		fragments.Add("(", ClassificationKind.Punctuation);

		fragments.AddRange(Left);
		fragments.Add(TextFragment.Space);
		fragments.Add("left", ClassificationKind.Parameter);

		fragments.Add(",", ClassificationKind.Punctuation);
		fragments.Add(TextFragment.Space);

		fragments.AddRange(Right);
		fragments.Add(TextFragment.Space);
		fragments.Add("right", ClassificationKind.Parameter);

		fragments.Add(")", ClassificationKind.Punctuation);

		if (Result.IsNotVoid)
		{
			fragments.Add(":", ClassificationKind.Punctuation);
			fragments.Add(TextFragment.Space);
			fragments.AddRange(Result);
		}

		return fragments;
	}
	#endregion
}
