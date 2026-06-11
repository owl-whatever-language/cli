namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Types;

public sealed class FunctionType : ITypeInfo
{
	#region Properties
	public IFunctionInfo Function { get; }
	public string? Name => Function.Signature?.ToString() ?? "???";
	#endregion

	#region Constructors
	public FunctionType(IFunctionInfo function) => Function = function;
	#endregion

	#region Methods
	public override string ToString() => Function.ToString() ?? "???";
	public bool CanBeAssignedTo(ITypeInfo type) => type is FunctionType other && other.Function == Function;
	#endregion
}
