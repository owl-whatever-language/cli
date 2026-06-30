namespace OwlDomain.Owl.Code.Execution.Builtins.Standard;

using OwlDomain.Owl.Code.Execution.Builtins.Attributes;
using static Core.CoreBuiltins;

internal partial class StandardBuiltins
{
	[Name("print")]
	public static void Print(IExecutionContext context, Text text)
	{
		Console.WriteLine(text.Value);
	}
}
