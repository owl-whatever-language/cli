namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Symbols;

public sealed class FunctionSymbol : BaseSymbol
{
	#region Properties
	[DisallowNull]
	public IFunctionInfo? Function
	{
		get;
		set
		{
			if (field is not null)
				ThrowHelper.ThrowInvalidOperationException("The function was already set.");

			field = value;
		}
	}
	#endregion

	#region Constructors
	public FunctionSymbol(string? name) : base(name)
	{
	}
	public FunctionSymbol(IFunctionInfo function) : this(function.Signature?.Name)
	{
		Function = function;
	}
	#endregion
}
