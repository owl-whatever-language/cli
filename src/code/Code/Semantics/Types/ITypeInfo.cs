namespace OwlDomain.OWL.Code.Semantics.Types;

public interface ITypeInfo
{
	#region Methods
	bool CanAssignTo(ITypeInfo target);
	#endregion
}
