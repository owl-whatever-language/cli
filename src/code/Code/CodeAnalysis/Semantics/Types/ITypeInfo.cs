namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;

public interface ITypeInfo
{
	#region Methods
	bool CanAssignTo(ITypeInfo target);
	#endregion
}
