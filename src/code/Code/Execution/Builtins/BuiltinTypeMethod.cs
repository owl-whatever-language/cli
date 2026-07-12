namespace OwlDomain.Owl.Code.Execution.Builtins;

internal class BuiltinTypeMethod : ITypeMethod
{
	#region Properties
	public string Id { get; } = SymbolHelpers.GetNewId();

	public IType DeclaringType { get; }
	public BuiltinFunction Function { get; }
	IFunction ITypeMethod.Function => Function;
	public string Name => Function.Name;
	#endregion

	#region Constructors
	public BuiltinTypeMethod(IType declaringType, BuiltinFunction function)
	{
		DeclaringType = declaringType;
		Function = function;
	}
	#endregion

	#region Methods
	public TextFragmentCollection GetDebugText()
	{
		return
		[
			..DeclaringType.GetDebugText(),
			new(".", ClassificationKind.Punctuation),
			..Function.GetDebugText()
		];
	}
	#endregion
}
