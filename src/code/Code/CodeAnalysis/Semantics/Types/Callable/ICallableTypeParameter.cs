namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Callable;

public interface ICallableTypeParameter : IDebugTextFactory
{
	#region Properties
	int Index { get; }
	string? Name { get; }
	IType Type { get; }
	#endregion
}
