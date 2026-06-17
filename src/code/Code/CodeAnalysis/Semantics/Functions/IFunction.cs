namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions;

public interface IFunction : ISymbolTarget
{
	#region Properties
	ICallable? Callable { get; set; }
	#endregion
}

public sealed class Function : BaseSymbolTarget, IFunction
{
	#region Properties
	public override string Kind => "function";
	#endregion

	#region Methods
	public ICallable? Callable
	{
		get;
		set => Set(ref field, value);
	}
	#endregion
}
