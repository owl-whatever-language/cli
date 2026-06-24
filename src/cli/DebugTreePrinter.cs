namespace OwlDomain.Owl.CLI;

public class DebugTreePrinter : DefaultDebugTreePrinter
{
	#region Constructors
	public DebugTreePrinter(IClassificationStyles styles) : base(styles)
	{
	}
	#endregion

	#region Functions
	public static void Print(IDebugTree tree, IClassificationStyles? styles = null)
	{
		Tree styled = Convert(tree, styles);
		AnsiConsole.Write(styled);
	}
	public static Tree Convert(IDebugTree tree, IClassificationStyles? styles = null)
	{
		styles ??= OwlClassificationStyles.Instance;
		DefaultDebugTreePrinter printer = new DebugTreePrinter(styles);

		return printer.Convert(tree);
	}

	public static void Print(IDebugTreeObject obj, IClassificationStyles? styles = null)
	{
		Tree styled = Convert(obj, styles);
		AnsiConsole.Write(styled);
	}
	public static Tree Convert(IDebugTreeObject obj, IClassificationStyles? styles = null)
	{
		styles ??= OwlClassificationStyles.Instance;
		DefaultDebugTreePrinter printer = new DebugTreePrinter(styles);

		return printer.Convert(obj);
	}
	#endregion
}
