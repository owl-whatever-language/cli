namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Targets;

public interface IFunctionParameter : INamedSymbolTarget
{
	#region Properties
	ITypeInfo? Type { get; set; }
	#endregion
}

public class FunctionParameter : BaseNamedSymbolTarget, IFunctionParameter
{
	#region Properties
	public override string Kind => "parameter";
	public ITypeInfo? Type
	{
		get;
		set => Set(ref field, value);
	}
	#endregion

	#region Constructors
	public FunctionParameter(string? name) : base(name) { }
	#endregion
}
