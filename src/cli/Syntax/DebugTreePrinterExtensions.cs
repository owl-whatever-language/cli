namespace OwlDomain.Owl.CLI.Syntax;

public static class DebugTreePrinterExtensions
{
	extension(DefaultDebugTreePrinter)
	{
		#region Functions
		public static Tree Convert(IDebugTree tree) => DefaultDebugTreePrinter.Convert(tree, OwlStyling.Default);
		public static Tree Convert(IDebugTreeObject obj) => DefaultDebugTreePrinter.Convert(obj, OwlStyling.Default);
		#endregion
	}

	extension(IDebugTree tree)
	{
		#region Methods
		public void Print() => DefaultDebugTreePrinter.Print(tree, OwlStyling.Default);
		#endregion
	}
	extension(IDebugTreeObject obj)
	{
		#region Methods
		public void Print() => DefaultDebugTreePrinter.Print(obj, OwlStyling.Default);
		#endregion
	}
}
