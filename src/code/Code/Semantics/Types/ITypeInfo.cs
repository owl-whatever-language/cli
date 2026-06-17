namespace OwlDomain.Owl.Code.Semantics.Types;

public interface ITypeInfo
{
	#region Methods
	bool CanAssignTo(ITypeInfo target);
	#endregion
}
