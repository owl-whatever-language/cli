namespace OwlDomain.Owl.CLI;

public class DebugTreePrinter : DefaultDebugTreePrinter
{
	#region Constructors
	public DebugTreePrinter(IClassificationStyling styles) : base(styles)
	{
	}
	#endregion

	#region Functions
	public static void Print(IDebugTree tree, IClassificationStyling? styles = null)
	{
		Tree styled = Convert(tree, styles);
		AnsiConsole.Write(styled);
	}
	public static Tree Convert(IDebugTree tree, IClassificationStyling? styles = null)
	{
		styles ??= OwlStyling.Default;
		DefaultDebugTreePrinter printer = new DebugTreePrinter(styles);

		return printer.Convert(tree);
	}

	public static void Print(IDebugTreeObject obj, IClassificationStyling? styles = null)
	{
		Tree styled = Convert(obj, styles);
		AnsiConsole.Write(styled);
	}
	public static Tree Convert(IDebugTreeObject obj, IClassificationStyling? styles = null)
	{
		styles ??= OwlStyling.Default;
		DefaultDebugTreePrinter printer = new DebugTreePrinter(styles);

		return printer.Convert(obj);
	}
	#endregion
}
