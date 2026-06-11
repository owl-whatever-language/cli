namespace OwlDomain.Owl.CLI.CodeAnalysis.Semantics.Types;

public interface ITypeInfo : ISymbolTarget
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

public sealed class TypeInfo : BaseSymbolTarget, ITypeInfo
{
	#region Properties
	public override string Kind => "type";

	public string? Name
	{
		get;
		set => Set(ref field, value);
	}
	#endregion

	#region Constructors
	public TypeInfo(string? name = null)
	{
		Name = name;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public bool CanBeAssignedTo(ITypeInfo type) => type == this;
	public override string ToString() => Name ?? "???";
	#endregion
}
