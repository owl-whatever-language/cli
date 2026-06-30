namespace OwlDomain.Owl.Code.Execution.Builtins;

internal class BuiltinType : INamedType
{
	#region Properties
	public string Name { get; }
	public Type Type { get; }
	#endregion

	#region Constructors
	public BuiltinType(string name, Type type)
	{
		Name = name;
		Type = type;
	}
	#endregion

	#region Methods
	public InterpreterValue CreateInstance(object? value)
	{
		object? instance = Activator.CreateInstance(Type, [value]);
		return new(this, instance);
	}
	public bool CanAssignTo(IType target) => Equals(target) || target == SpecialTypes.Void;
	public bool Equals(IType? other) => ReferenceEquals(this, other);
	public override string ToString() => Name;
	TextFragmentCollection IDebugTreePrintable.GetFragments() => [new(Name, ClassificationKind.Type)];
	#endregion
}
