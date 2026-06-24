namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;

public static class SpecialTypes
{
	#region Properties
	public static ErrorType Error => ErrorType.Instance;
	public static UnknownType Unknown => UnknownType.Instance;
	public static VoidType Void => VoidType.Instance;
	#endregion
}
