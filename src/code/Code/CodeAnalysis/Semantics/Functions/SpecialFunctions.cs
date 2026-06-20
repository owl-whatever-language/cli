namespace OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions;

public static class SpecialFunctions
{
	#region Properties
	public static IFunction Print { get; } = GetPrint();
	#endregion

	#region Functions
	public static IEnumerable<IFunction> GetAll() => [Print];
	private static IFunction GetPrint()
	{
		Function function = new("print");
		CallableReturn @return = new CallableReturn(SpecialTypes.Void).Lock();
		CallableParameter parameter = new CallableParameter(SpecialTypes.Text, "text").WithSymbol("text").Lock();
		Callable callable = new(function, [parameter], @return);

		return function.WithCallable(callable).WithSymbol("print").Lock();
	}
	#endregion
}
