namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Functions;

public interface IFunctionInfo
{
	#region Properties
	FunctionType AsType { get; }
	string? Name { get; }
	#endregion
}

public sealed class ImmutableFunctionInfo : IFunctionInfo
{
	#region Properties
	public FunctionType AsType { get; }
	public string? Name { get; }
	#endregion

	#region Constructors
	public ImmutableFunctionInfo(string? name)
	{
		Name = name;
		AsType = new(this);
	}
	#endregion
}

public sealed class MutableFunctionInfo : IFunctionInfo
{
	#region Properties
	public FunctionType AsType { get; }
	public string? Name { get; set; }
	#endregion

	#region Constructors
	public MutableFunctionInfo()
	{
		AsType = new(this);
	}
	#endregion

	#region Methods
	public ImmutableFunctionInfo ToMutable() => new(Name);
	#endregion
}
