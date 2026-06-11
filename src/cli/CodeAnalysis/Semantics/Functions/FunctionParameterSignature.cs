namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Functions;

public sealed class FunctionParameterSignature
{
	#region Properties
	public ITypeInfo Type { get; }
	public string? Name { get; }
	#endregion

	#region Constructors
	public FunctionParameterSignature(ITypeInfo type, string? name)
	{
		Type = type;
		Name = name;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override string ToString()
	{
		if (Name is not null)
			return $"{Type} {Name}";

		return Type.ToString() ?? "???";
	}
	#endregion
}
