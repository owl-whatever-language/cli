namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;

public sealed class ErrorType : IType
{
	#region Properties
	public static ErrorType Instance { get; } = new();
	#endregion

	#region Constructors
	private ErrorType() { }
	#endregion

	#region Methods
	public bool CanAssignTo(IType target) => false;
	public bool Equals(IType? other) => false;
	public override bool Equals(object? obj) => false;
	public override int GetHashCode() => base.GetHashCode();
	public override string ToString() => "error";
	#endregion
}
