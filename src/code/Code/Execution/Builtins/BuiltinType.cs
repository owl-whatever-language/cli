namespace OwlDomain.Owl.Code.Execution.Builtins;

internal class BuiltinType : INamedType
{
	#region Nested types
	public delegate InterpreterValue BackingConstructorDelegate<in T>(T backing);
	#endregion

	#region Properties
	public string Id { get; } = SymbolHelpers.GetNewId();
	public string Name { get; }
	public List<ITypeProperty> Properties { get; } = [];
	public List<BuiltinFunction> Operations { get; } = [];
	public BackingConstructorDelegate<object?>? BackingConstructor { get; set; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IReadOnlyList<ITypeProperty> IType.Properties => Properties;
	#endregion

	#region Constructors
	public BuiltinType(string name)
	{
		Name = name;
	}
	#endregion

	#region Methods
	public bool FindOperation(IType left, IType right, OperatorKind @operator, [NotNullWhen(true)] out IFunction? function)
	{
		Debug.Assert(left == this || right == this);

		foreach (BuiltinFunction operation in Operations)
		{
			if (operation.Parameters.Count is not 2)
				continue;

			if (operation.Name != @operator.ToString())
				continue;

			IFunctionParameter leftParameter = operation.Parameters[0];
			IFunctionParameter rightParameter = operation.Parameters[1];

			if (leftParameter.Type != left || rightParameter.Type != right)
				continue;

			function = operation;
			return true;
		}

		function = default;
		return false;
	}
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
