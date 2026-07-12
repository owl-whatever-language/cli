namespace OwlDomain.Owl.Code.Execution.Builtins.Standard;

using System.IO;
using OwlDomain.Owl.Code.Execution.Builtins.Attributes;
using static Core.CoreBuiltins;

internal partial class StandardBuiltins
{
	#region Print
	[Name("print")] public static void Print(Text value) => Console.Write(value.ToString());
	[Name("print")] public static void Print(Bool value) => Console.Write(value.ToString());
	[Name("print")] public static void Print(Int value) => Console.Write(value.ToString());
	[Name("print")] public static void Print(Num value) => Console.Write(value.ToString());
	#endregion

	#region Print line
	[Name("println")] public static void Println(Text value) => Console.WriteLine(value.ToString());
	[Name("println")] public static void Println(Bool value) => Console.WriteLine(value.ToString());
	[Name("println")] public static void Println(Int value) => Console.WriteLine(value.ToString());
	[Name("println")] public static void Println(Num value) => Console.WriteLine(value.ToString());
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
	[Name("input")] public static Text Input() => Input(new(string.Empty));
	#endregion

	#region IO
	[Name("readFile")]
	public static Text ReadFile(Text path)
	{
		string contents = File.ReadAllText(path.Value);
		return new(contents);
	}

	[Name("writeFile")] public static void WriteFile(Text path, Text value) => File.WriteAllText(path.Value, value.Value);
	[Name("appendFile")] public static void AppendFile(Text path, Text value) => File.AppendAllText(path.Value, value.Value);

	[Name("pathAppend")]
	public static Text PathGetParent(Text path)
	{
		string? parent = Path.GetDirectoryName(path.Value);
		return new(parent ?? string.Empty);
	}

	[Name("pathAppend")]
	public static Text PathAppend(Text path, Text item)
	{
		string trimmed = item.Value
			.Trim(Path.DirectorySeparatorChar)
			.Trim(Path.AltDirectorySeparatorChar);

		string result = Path.Combine(path.Value, trimmed);

		return new(result);
	}
	#endregion

	#region Resolve functions
	[Ignore]
	public static void Resolve(BuiltinContext context)
	{
		context.AddFunction<Text>("print", "value", Print);
		context.AddFunction<Bool>("print", "value", Print);
		context.AddFunction<Int>("print", "value", Print);
		context.AddFunction<Num>("print", "value", Print);

		context.AddFunction<Text>("println", "value", Println);
		context.AddFunction<Bool>("println", "value", Println);
		context.AddFunction<Int>("println", "value", Println);
		context.AddFunction<Num>("println", "value", Println);
		context.AddFunction("println", Println);

		context.AddFunction<Text>("input", Input);
		context.AddFunction<Text, Text>("input", "value", Input);

		context.AddFunction<Text, Text>("writeFile", "path", "value", WriteFile);
		context.AddFunction<Text, Text>("appendFile", "path", "value", AppendFile);
		context.AddFunction<Text, Text>("readFile", "path", ReadFile);

		context.AddFunction<Text, Text>("pathGetParent", "path", PathGetParent);
		context.AddFunction<Text, Text, Text>("pathAppend", "path", "value", PathAppend);
	}
	#endregion
}
