namespace OwlDomain.Owl.Code.Execution.Builtins.Standard;

using OwlDomain.Owl.Code.Execution.Builtins.Attributes;

using IOPath = System.IO.Path;
using static Core.CoreBuiltins;
using System.IO;
using System.Text;

internal partial class StandardBuiltins
{
	#region Types
	[Name("Path")]
	public sealed class Path
	{
		#region Properties
		[Ignore] public string Value { get; }
		#endregion

		#region Constructors
		public Path(string value) => Value = TrimEnd(value);
		#endregion

		#region Helpers
		[Ignore]
		private static string TrimEnd(string value)
		{
			if (IOPath.GetPathRoot(value) == value)
				return value;

			return value
				.TrimEnd(IOPath.DirectorySeparatorChar)
				.TrimEnd(IOPath.AltDirectorySeparatorChar);
		}
		[Ignore]
		private static string Trim(string value)
		{
			return value
				.Trim(IOPath.DirectorySeparatorChar)
				.TrimEnd(IOPath.AltDirectorySeparatorChar);
		}
		#endregion

		#region Operators
		[Operator]
		public static Path Add(Path value, Text addition)
		{
			string left = TrimEnd(value.Value);
			string right = Trim(addition.Value);

			return new(left + right);
		}

		public static Path Divide(Path value, Text child)
		{
			string left = TrimEnd(value.Value);
			string right = Trim(child.Value);

			string result = IOPath.Combine(left, right);
			return new(result);
		}
		#endregion

		#region Functions
		[Name("createPath")] public static Path PathFrom(Text value) => new(TrimEnd(value.Value));
		[Name("getCurrentPath")] public static Path GetCurrentPath() => new(Environment.CurrentDirectory);
		#endregion

		#region Methods
		[Method("toText")] public static Text ToText(Path value) => new(value.Value);
		[Method("getFull")] public static Path GetFull(Path value) => new(IOPath.GetFullPath(value.Value));
		[Method("getFull")] public static Path GetFull(Path value, Path @base) => new(IOPath.GetFullPath(value.Value, @base.Value));
		[Method("getRelative")] public static Path GetRelative(Path value, Path @base) => new(IOPath.GetRelativePath(@base.Value, value.Value));
		[Method("getParent")] public static Path GetParent(Path value) => new(IOPath.GetDirectoryName(value.Value) ?? string.Empty);

		[Method("readText")]
		public static Text ReadText(Path value)
		{
			string text = File.ReadAllText(value.Value, Encoding.UTF8);
			return new(text);
		}

		[Method("writeText")]
		public static void WriteText(Path value, Text contents)
		{
			EnsureParentExists(value);
			File.WriteAllText(value.Value, contents.Value, Encoding.UTF8);
		}

		[Method("appendText")]
		public static void AppendText(Path value, Text contents)
		{
			File.AppendAllText(value.Value, contents.Value, Encoding.UTF8);
		}

		[Method("ensureParentExists")]
		public static void EnsureParentExists(Path value)
		{
			Path parent = GetParent(GetFull(value));
			CreateDirectory(parent);
		}
		[Method("createDirectory")] public static void CreateDirectory(Path value) => Directory.CreateDirectory(value.Value);
		[Method("createFile")] public static void CreateFile(Path value) => WriteText(value, new(string.Empty));
		#endregion

		#region Properties
		[Property("Exists")]
		public static Bool Exists(Path value)
		{
			if (File.Exists(value.Value))
				return new(true);

			if (Directory.Exists(value.Value))
				return new(true);

			return new(false);
		}
		#endregion
	}
	#endregion

	#region Print
	[Name("print")] public static void Print(Text value) => Console.Write(value.ToString());
	[Name("print")] public static void Print(Bool value) => Console.Write(value.ToString());
	[Name("print")] public static void Print(Int value) => Console.Write(value.ToString());
	[Name("print")] public static void Print(UInt value) => Console.Write(value.ToString());
	[Name("print")] public static void Print(Num value) => Console.Write(value.ToString());
	#endregion

	#region Print line
	[Name("println")] public static void Println(Text value) => Console.WriteLine(value.ToString());
	[Name("println")] public static void Println(Bool value) => Console.WriteLine(value.ToString());
	[Name("println")] public static void Println(Int value) => Console.WriteLine(value.ToString());
	[Name("println")] public static void Println(UInt value) => Console.Write(value.ToString());
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

	#region Resolve functions
	[Ignore]
	public static void Resolve(BuiltinContext context)
	{
		context.AddType<string, Path>("path", b => new(b));

		ResolveFunctions(context);

		ResolvePath(context);
	}

	private static void ResolveFunctions(BuiltinContext context)
	{
		ResolvePrintFunctions(context);

		#region Input
		context.AddFunction("input", Input);
		context.AddFunction<Text, Text>("input", "value", Input);
		#endregion
	}
	private static void ResolvePrintFunctions(BuiltinContext context)
	{
		#region Print
		context.AddFunction<Text>("print", "value", Print);
		context.AddFunction<Bool>("print", "value", Print);
		context.AddFunction<Int>("print", "value", Print);
		context.AddFunction<UInt>("print", "value", Print);
		context.AddFunction<Num>("print", "value", Print);
		#endregion

		#region Println
		context.AddFunction<Text>("println", "value", Println);
		context.AddFunction<Bool>("println", "value", Println);
		context.AddFunction<Int>("println", "value", Println);
		context.AddFunction<UInt>("println", "value", Println);
		context.AddFunction<Num>("println", "value", Println);
		context.AddFunction("println", Println);
		#endregion
	}

	private static void ResolvePath(BuiltinContext context)
	{
		BuiltinType type = context[typeof(Path)];

		#region Operators
		context.AddBinary<Path, Text, Path>(type, OperatorKind.Add, Path.Add);
		context.AddBinary<Path, Text, Path>(type, OperatorKind.Divide, Path.Divide);
		#endregion

		#region Functions
		context.AddFunction<Text, Path>("createPath", "value", Path.PathFrom);
		context.AddFunction("getCurrentPath", Path.GetCurrentPath);
		#endregion

		#region Methods
		context.AddMethod<Path, Text>("toText", Path.ToText);
		context.AddMethod<Path, Path>("getFull", Path.GetFull);
		context.AddMethod<Path, Path, Path>("getFull", "base", Path.GetFull);
		context.AddMethod<Path, Path>("getParent", Path.GetParent);
		context.AddMethod<Path, Path, Path>("getRelative", "base", Path.GetRelative);
		context.AddMethod<Path, Text>("readText", Path.ReadText);
		context.AddMethod<Path, Text>("writeText", "contents", Path.WriteText);
		context.AddMethod<Path, Text>("appendText", "contents", Path.AppendText);
		context.AddMethod<Path>("ensureParentExists", Path.EnsureParentExists);
		context.AddMethod<Path>("createDirectory", Path.CreateDirectory);
		context.AddMethod<Path>("createFile", Path.CreateFile);
		#endregion

		#region Properties
		context.AddProperty<Path, Bool>("Exists", Path.Exists);
		#endregion
	}
	#endregion
}
