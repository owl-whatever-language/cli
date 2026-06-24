namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions.Declared;

public interface IDeclaredFunctionReturn : IFunctionReturn
{
	#region Properties
	IConcreteFunctionReturnSyntax Declaration { get; set; }
	new IType Type { get; set; }
	#endregion
}

public sealed class DeclaredFunctionReturn : IDeclaredFunctionReturn
{
	#region Properties
	public IConcreteFunctionReturnSyntax Declaration
	{
		get;
		set
		{
			field?.ThrowIfInvalidShadow(value);
			field = value;
		}
	}
	public IType Type
	{
		get;
		set
		{
			field?.ThrowIfInvalidShadow(value);
			field = value;
		}
	}
	public ICallableFunctionReturn AsCallable { get; }
	#endregion

	#region Constructors
	public DeclaredFunctionReturn(IConcreteFunctionReturnSyntax declaration)
	{
		Declaration = declaration;
		Type = SpecialTypes.Unknown;
		AsCallable = new CallableFunctionReturn(this);
	}
	#endregion
}
