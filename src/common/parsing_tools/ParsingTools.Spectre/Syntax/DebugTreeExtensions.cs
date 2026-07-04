namespace OwlDomain.ParsingTools.Syntax;

public static class DebugTreeExtensions
{
	extension(IDebugTree tree)
	{
		#region Methods
		public Tree Convert(IClassificationStyling styles) => new DefaultDebugTreePrinter(styles).Convert(tree);
		public void Print(IClassificationStyling styles)
		{
			Tree styled = Convert(tree, styles);

			using (AnsiConsole.WithBackground(styles))
				AnsiConsole.Write(styled);
		}
		#endregion
	}
	extension(IDebugTreeObject obj)
	{
		public Tree Convert(IClassificationStyling styles) => new DefaultDebugTreePrinter(styles).Convert(obj);
		public void Print(IClassificationStyling styles)
		{
			Tree styled = Convert(obj, styles);

			using (AnsiConsole.WithBackground(styles))
				AnsiConsole.Write(styled);
		}
	}
}
