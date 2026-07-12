namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Members;

public interface ITypeProperty : ITypeMember
{
	#region Properties
	IType Type { get; }
	#endregion
}
