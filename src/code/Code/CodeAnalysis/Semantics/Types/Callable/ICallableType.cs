namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Callable;

public interface ICallableType : IType, IEquatable<ICallableType>
{
	#region Properties
	IReadOnlyList<ICallableTypeParameter> Parameters { get; }
	ICallableTypeReturn Return { get; }
	#endregion
}
