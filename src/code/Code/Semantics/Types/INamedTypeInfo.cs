namespace OwlDomain.Owl.Code.Semantics.Types;

public interface INamedTypeInfo : ITypeInfo
{
	#region Properties
	string? Name { get; }
	#endregion
}

public sealed class NamedTypeInfo : BaseSymbolTarget, INamedTypeInfo
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
	public NamedTypeInfo() { }
	public NamedTypeInfo(string name) => Name = name;
	#endregion

	#region Methods
	public bool CanAssignTo(ITypeInfo target) => target == this;
	#endregion
}
