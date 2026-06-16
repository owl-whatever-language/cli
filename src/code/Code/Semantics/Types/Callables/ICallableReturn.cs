namespace OwlDomain.OWL.Code.Semantics.Types.Callables;

public interface ICallableReturn
{
	#region Properties
	ITypeInfo Type { get; }
	#endregion

	#region Methods
	bool CanAssignTo(ICallableReturn target);
	#endregion
}

public sealed class CallableReturn : ICallableReturn
{
	#region Properties
	public ITypeInfo Type { get; }
	#endregion

	#region Constructors
	public CallableReturn(ITypeInfo type) => Type = type;
	#endregion

	#region Methods
	public bool CanAssignTo(ICallableReturn target)
	{
		if (target == SpecialTypes.Void)
			return true;

		return Type.CanAssignTo(target.Type);
	}
	#endregion
}
