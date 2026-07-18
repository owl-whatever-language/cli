namespace OwlDomain.Owl.Code.Execution.Builtins;

internal class BuiltinType : INamedType
{
	#region Nested types
	public delegate InterpreterValue BackingConstructorDelegate<in T>(T backing);
	#endregion

	#region Properties
	public string Id { get; } = SymbolHelpers.GetNewId();
	public string Name { get; }
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
	public TextFragmentCollection GetDebugText() => [new(Name, ClassificationKind.Type)];
	#endregion
}
