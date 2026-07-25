namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Members;

public interface ITypeMethod : ITypeMember
{
	#region Properties
	IFunction Function { get; }
	#endregion
}
