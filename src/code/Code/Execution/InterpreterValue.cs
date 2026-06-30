namespace OwlDomain.Owl.Code.Execution;

public readonly struct InterpreterValue
{
	#region Properties
	public static InterpreterValue Void => new(SpecialTypes.Void, null);
	public IType Type { get; }
	public object? Value { get; }
	#endregion

	#region Constructors
	public InterpreterValue(IType type, object? value)
	{
		Type = type;
		Value = value;
	}
	#endregion
}
