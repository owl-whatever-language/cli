namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;

public interface IBinaryOperator : IDebugTextFactory
{
	#region Properties
	OperatorKind Kind { get; }
	IType Left { get; }
	IType Right { get; }
	IType Result { get; }
	IFunction AsFunction { get; }
	#endregion
}
