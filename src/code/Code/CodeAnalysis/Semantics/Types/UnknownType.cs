namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;

public sealed class UnknownType : IType
{
	#region Properties
	public static UnknownType Instance { get; } = new();
	#endregion

	#region Constructors
	private UnknownType() { }
	#endregion

	#region Methods
	public bool CanAssignTo(IType target) => false;
	public bool Equals(IType? other) => false;
	public override bool Equals(object? obj) => false;
	public override int GetHashCode() => base.GetHashCode();
	public override string ToString() => "unknown";
	#endregion
}
