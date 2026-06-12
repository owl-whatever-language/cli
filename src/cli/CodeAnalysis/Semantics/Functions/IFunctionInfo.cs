namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Functions;

public interface IFunctionInfo : ISymbolTarget, ITypeTarget
{
	#region Properties
	FunctionType AsType { get; }
	FunctionSignature? Signature { get; }
	ITypeInfo? ITypeTarget.Type => AsType;
	#endregion
}

public sealed class FunctionInfo : BaseSymbolTarget, IFunctionInfo
{
	#region Properties
	public override string Kind => "function";
	public FunctionType AsType { get; }
	public FunctionSignature? Signature
	{
		get;
		set => Set(ref field, value);
	}
	#endregion

	#region Constructors
	public FunctionInfo(FunctionSignature? signature = null)
	{
		AsType = new(this);
		Signature = signature;
	}
	#endregion

	#region Methods
	public override string ToString() => Signature?.ToString() ?? "???";
	#endregion
}
