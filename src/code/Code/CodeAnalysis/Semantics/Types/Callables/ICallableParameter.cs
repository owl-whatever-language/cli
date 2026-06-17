namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Callables;

public interface ICallableParameter
{
	#region Properties
	ITypeInfo Type { get; }
	string? Name { get; }
	#endregion

	#region Methods
	bool CanAssignTo(ICallableParameter target);
	#endregion
}

public sealed class CallableParameter : ICallableParameter
{
	#region Properties
	public ITypeInfo Type { get; }
	public string? Name { get; }
	#endregion

	#region Constructors
	public CallableParameter(ITypeInfo type, string? name = null)
	{
		Type = type;
		Name = name;
	}
	#endregion

	#region Methods
	public bool CanAssignTo(ICallableParameter target)
	{
		return Type.CanAssignTo(target.Type);
	}
	#endregion
}
