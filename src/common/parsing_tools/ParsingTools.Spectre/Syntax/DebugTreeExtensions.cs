namespace OwlDomain.ParsingTools.Syntax;

public static class DebugTreeExtensions
{
	extension(IDebugTree tree)
	{
		#region Methods
		public Tree Convert(IClassificationStyles styles) => new DefaultDebugTreePrinter(styles).Convert(tree);
		public void Print(IClassificationStyles styles)
		{
			Tree styled = Convert(tree, styles);
			AnsiConsole.Write(styled);
		}
		#endregion
	}
	extension(IDebugTreeObject obj)
	{
		public Tree Convert(IClassificationStyles styles) => new DefaultDebugTreePrinter(styles).Convert(obj);
		public void Print(IClassificationStyles styles)
		{
			Tree styled = Convert(obj, styles);
			AnsiConsole.Write(styled);
		}
	}
}
