namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Types;

public interface ITypeInfo
{
	#region Properties
	string? Name { get; }
	#endregion

	#region Methods
	/// <summary>Checks whether a value of the current type can assigned to the given <paramref name="type"/>.</summary>
	/// <param name="type">The type to assign a value of the current type.</param>
	/// <returns><see langword="true"/> if a value of the current type can be assigned to the given <paramref name="type"/>.</returns>
	bool CanBeAssignedTo(ITypeInfo type);
	#endregion
}

public sealed class ImmutableTypeInfo : ITypeInfo
{
	#region Properties
	public string? Name { get; }
	#endregion

	#region Constructors
	public ImmutableTypeInfo(string? name)
	{
		Name = name;
	}
	#endregion

	#region Methods
	public bool CanBeAssignedTo(ITypeInfo type) => type.Name == Name;
	public override string ToString() => Name ?? "???";
	#endregion
}

public sealed class MutableTypeInfo : ITypeInfo
{
	#region Properties
	public string? Name { get; set; }
	#endregion

	#region Methods
	public ImmutableTypeInfo ToImmutable() => new(Name);
	#endregion

	#region Methods
	public bool CanBeAssignedTo(ITypeInfo type) => type.Name == Name;
	public override string ToString() => Name ?? "???";
	#endregion
}
