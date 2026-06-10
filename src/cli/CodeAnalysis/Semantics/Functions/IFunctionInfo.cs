namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Functions;

public interface IFunctionInfo
{
	#region Properties
	string? Name { get; }
	#endregion
}

public sealed class ImmutableFunctionInfo : IFunctionInfo
{
	#region Properties
	public string? Name { get; }
	#endregion

	#region Constructors
	public ImmutableFunctionInfo(string? name)
	{
		Name = name;
	}
	#endregion
}

public sealed class MutableFunctionInfo : IFunctionInfo
{
	#region Properties
	public string? Name { get; set; }
	#endregion

	#region Methods
	public ImmutableFunctionInfo ToMutable() => new(Name);
	#endregion
}
