namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Functions;

public class FunctionReturnSignature
{
	#region Properties
	public ITypeInfo Type { get; }
	#endregion

	#region Constructors
	public FunctionReturnSignature(ITypeInfo type)
	{
		Type = type;
	}
	#endregion

	#region Methods
	public override string ToString() => Type.ToString() ?? "???";
	#endregion
}
