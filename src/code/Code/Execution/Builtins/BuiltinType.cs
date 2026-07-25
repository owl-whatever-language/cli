namespace OwlDomain.Owl.Code.Execution.Builtins;

internal class BuiltinType : BaseSymbol, INamedType
{
	#region Nested types
	public delegate InterpreterValue BackingConstructorDelegate<in T>(T backing);
	#endregion

	#region Properties
	public override string Name { get; }
	public override ClassificationKind Classification => ClassificationKind.Type;
	public List<ITypeMember> Members { get; } = [];
	public IReadOnlyCollection<ITypeProperty> Properties => Members.OfType<ITypeProperty>().ToArray();
	public IReadOnlyCollection<ITypeMethod> Methods => Members.OfType<ITypeMethod>().ToArray();

	public List<BuiltinBinaryOperator> BinaryOperators { get; } = [];
	public BackingConstructorDelegate<object?>? BackingConstructor { get; set; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyCollection<ITypeMember> IType.Members => Members;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyCollection<IBinaryOperator> IType.BinaryOperators => BinaryOperators;
	#endregion

	#region Constructors
	public BuiltinType(string name)
	{
		Name = name;
	}
	#endregion

	#region Methods
	public InterpreterValue CreateInstance(object? value)
	{
		if (BackingConstructor is null)
			ThrowHelper.ThrowInvalidOperationException($"The backing constructor for the type '{Name}' hasn't been set yet.");

		InterpreterValue instance = BackingConstructor.Invoke(value);
		return instance;
	}
	public bool CanAssignTo(IType target) => Equals(target);
	public bool Equals(IType? other) => ReferenceEquals(this, other);
	public override string ToString() => Name;
	public override TextFragmentCollection GetDebugText() => [new(Name, ClassificationKind.Type)];
	#endregion
}
