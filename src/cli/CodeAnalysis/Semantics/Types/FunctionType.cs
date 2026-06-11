namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Types;

public sealed class FunctionType : ITypeInfo
{
	#region Properties
	public string Kind => "function type";
	public IFunctionInfo Function { get; }
	public string? Name => Function.Signature?.ToString() ?? "???";
	public bool IsMutable => Function.IsMutable;
	public ISymbol Symbol => Function.Symbol;
	#endregion

	#region Constructors
	public FunctionType(IFunctionInfo function) => Function = function;
	#endregion

	#region Methods
	public override string ToString() => Function.ToString() ?? "???";
	public bool CanBeAssignedTo(ITypeInfo type) => type is FunctionType other && other.Function == Function;
	#endregion
}
