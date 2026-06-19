namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;

public interface INamedTypeInfo : ITypeInfo, INamedSymbolTarget
{
}

public sealed class NamedTypeInfo : BaseNamedSymbolTarget, INamedTypeInfo
{
	#region Properties
	public override string Kind => "type";
	#endregion

	#region Constructors
	public NamedTypeInfo(string? name) : base(name) { }
	#endregion

	#region Methods
	public bool CanAssignTo(ITypeInfo target) => target == this;
	#endregion
}
