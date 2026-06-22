namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Callables;

public interface ICallableParameter : INamedSymbolTarget
{
	#region Properties
	ITypeInfo? Type { get; set; }
	IFunctionParameter? Parameter { get; set; }
	#endregion

	#region Methods
	bool CanAssignTo(ICallableParameter target);
	#endregion
}

public sealed class CallableParameter : BaseNamedSymbolTarget, ICallableParameter
{
	#region Properties
	public override string Kind => "parameter";
	public ITypeInfo? Type
	{
		get;
		set => Set(ref field, value);
	}
	public IFunctionParameter? Parameter
	{
		get;
		set => Set(ref field, value);
	}
	#endregion

	#region Constructors
	public CallableParameter(string? name = null) : base(name) { }
	public CallableParameter(ITypeInfo type, string? name = null) : this(name)
	{
		Type = type;
	}
	#endregion

	#region Methods
	public bool CanAssignTo(ICallableParameter target)
	{
		if (Type is null || target.Type is null)
			return false;

		return Type.CanAssignTo(target.Type);
	}

	public override string ToString() => Type?.ToString() ?? "???";
	#endregion
}
