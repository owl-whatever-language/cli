namespace OwlDomain.Owl.Code.Execution.Builtins;

internal class BuiltinTypeMethod : BaseSymbol, ITypeMethod
{
	#region Properties
	public override string Name => Function.Name;
	public override ClassificationKind Classification => ClassificationKind.TypeMethod;
	public IType DeclaringType { get; }
	public BuiltinFunction Function { get; }
	IFunction ITypeMethod.Function => Function;
	#endregion

	#region Constructors
	public BuiltinTypeMethod(IType declaringType, BuiltinFunction function)
	{
		char first = function.Name.First();
		if (first == char.ToUpper(first))
			ThrowHelper.ThrowInvalidOperationException($"Methods should be camelCase, but instead got ({function.Name}).");

		DeclaringType = declaringType;
		Function = function;
	}
	#endregion

	#region Methods
	public override TextFragmentCollection GetDebugText()
	{
		return
		[
			..DeclaringType.GetDebugText(),
			TextFragment.Period,
			..Function.GetDebugText()
		];
	}
	#endregion
}
