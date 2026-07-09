namespace OwlDomain.Owl.Code.Execution.Builtins.Standard;

using OwlDomain.Owl.Code.Execution.Builtins.Attributes;
using static Core.CoreBuiltins;

internal partial class StandardBuiltins
{
	#region Print
	[Name("print")] public static void Print(Text value) => Console.Write(value.ToString());
	[Name("print")] public static void Print(Bool value) => Console.Write(value.ToString());
	[Name("print")] public static void Print(Int value) => Console.Write(value.ToString());
	#endregion

	#region Print line
	[Name("println")] public static void Println(Text value) => Console.WriteLine(value.ToString());
	[Name("println")] public static void Println(Bool value) => Console.WriteLine(value.ToString());
	[Name("println")] public static void Println(Int value) => Console.WriteLine(value.ToString());
	[Name("println")] public static void Println() => Console.WriteLine();
	#endregion

	#region Input
	[Name("input")]
	public static Text Input(Text prompt)
	{
		Console.Write(prompt.Value);
		string input = Console.ReadLine() ?? string.Empty;

		return new(input);
	}
	#endregion
}
