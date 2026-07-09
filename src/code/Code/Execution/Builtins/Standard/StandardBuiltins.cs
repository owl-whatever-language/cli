namespace OwlDomain.Owl.Code.Execution.Builtins.Standard;

using OwlDomain.Owl.Code.Execution.Builtins.Attributes;
using static Core.CoreBuiltins;

internal partial class StandardBuiltins
{
	[Name("print")]
	public static void Print(Text text)
	{
		Console.Write(text.Value);
	}

	[Name("println")]
	public static void Println(Text text)
	{
		Console.WriteLine(text.Value);
	}

	[Name("input")]
	public static Text Input(Text prompt)
	{
		Console.Write(prompt.Value);
		string input = Console.ReadLine() ?? string.Empty;

		return new(input);
	}
}
