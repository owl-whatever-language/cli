namespace OwlDomain.Owl.Code.Execution.Builtins;

internal class BuiltinTypeProperty : ITypeProperty
{
	#region Nested types
	public delegate InterpreterValue GetPropertyDelegate(InterpreterValue instance);
	#endregion

	#region Properties
	public string Id { get; } = SymbolHelpers.GetNewId();

	public IType DeclaringType { get; }
	public IType Type { get; }
	public string Name { get; }
	public GetPropertyDelegate Getter { get; }
	#endregion

	#region Constructors
	public BuiltinTypeProperty(IType declaringType, IType type, string name, GetPropertyDelegate getter)
	{
		DeclaringType = declaringType;
		Type = type;
		Name = name;
		Getter = getter;
	}
	#endregion

	#region Methods
	public TextFragmentCollection GetDebugText()
	{
		return
		[
			..DeclaringType.GetDebugText(),
			new(".", ClassificationKind.Punctuation),
			new(Name, ClassificationKind.TypeProperty),
			new(":", ClassificationKind.Punctuation),
			new(" ", ClassificationKind.Whitespace),
			..Type.GetDebugText()
		];
	}
	#endregion
}
