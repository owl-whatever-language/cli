namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Callables;

public interface ICallableParameter : INamedSymbolTarget
{
	#region Properties
	int Index { get; }
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
	public int Index { get; }
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
	public CallableParameter(int index, string? name = null) : base(name)
	{
		Index = index;
	}
	public CallableParameter(int index, ITypeInfo type, string? name = null) : this(index, name)
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
