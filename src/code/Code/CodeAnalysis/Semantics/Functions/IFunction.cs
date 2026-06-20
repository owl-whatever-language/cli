namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions;

public interface IFunction : INamedSymbolTarget
{
	#region Properties
	ICallable? Callable { get; set; }
	#endregion
}

public sealed class Function : BaseNamedSymbolTarget, IFunction
{
	#region Properties
	public override string Kind => "function";
	public ICallable? Callable
	{
		get;
		set => Set(ref field, value);
	}
	#endregion

	#region Constructors
	public Function(string? name) : base(name) { }
	#endregion

	#region Methods
	public Function WithCallable(ICallable callable)
	{
		Callable = callable;
		return this;
	}
	#endregion
}
