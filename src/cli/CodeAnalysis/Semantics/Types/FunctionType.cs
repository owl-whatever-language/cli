namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Types;

public sealed class FunctionType : ITypeInfo
{
	#region Properties
	public IFunctionInfo Function { get; }
	public string? Name => Function.Name;
	#endregion

	#region Constructors
	public FunctionType(IFunctionInfo function) => Function = function;
	#endregion

	#region Methods
	public override string ToString() => Name ?? "???";
	#endregion
}
