namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Functions;

public interface IFunctionInfo
{
	#region Properties
	FunctionType AsType { get; }
	FunctionSignature? Signature { get; }
	#endregion
}

public sealed class ImmutableFunctionInfo : IFunctionInfo
{
	#region Properties
	public FunctionType AsType { get; }
	public FunctionSignature? Signature { get; }
	#endregion

	#region Constructors
	public ImmutableFunctionInfo(FunctionSignature? signature)
	{
		AsType = new(this);
		Signature = signature;
	}
	#endregion

	#region Methods
	public override string ToString() => Signature?.ToString() ?? "???";
	#endregion
}

public sealed class MutableFunctionInfo : IFunctionInfo
{
	#region Properties
	public FunctionType AsType { get; }
	public FunctionSignature? Signature { get; set; }
	#endregion

	#region Constructors
	public MutableFunctionInfo()
	{
		AsType = new(this);
	}
	#endregion

	#region Methods
	public ImmutableFunctionInfo ToMutable() => new(Signature);
	public override string ToString() => Signature?.ToString() ?? "???";
	#endregion
}
