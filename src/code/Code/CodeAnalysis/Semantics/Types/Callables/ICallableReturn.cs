namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Callables;

public interface ICallableReturn
{
	#region Properties
	ITypeInfo? Type { get; set; }
	#endregion

	#region Methods
	bool CanAssignTo(ICallableReturn target);
	#endregion
}

public sealed class CallableReturn : BaseMutableTarget, ICallableReturn
{
	#region Properties
	public ITypeInfo? Type
	{
		get;
		set => Set(ref field, value);
	}
	#endregion

	#region Constructors
	public CallableReturn(ITypeInfo type) => Type = type;
	public CallableReturn() { }
	#endregion

	#region Methods
	public bool CanAssignTo(ICallableReturn target)
	{
		if (target == SpecialTypes.Void)
			return true;

		if (Type is null || target.Type is null)
			return false;

		return Type.CanAssignTo(target.Type);
	}
	#endregion
}
