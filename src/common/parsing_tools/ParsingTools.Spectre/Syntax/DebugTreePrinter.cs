namespace OwlDomain.ParsingTools.Syntax;

using Segment = AnsiMarkupSegment;
using Segments = IReadOnlyList<AnsiMarkupSegment>;

public class DefaultDebugTreePrinter
{
	#region Properties
	// Note(Nightowl): These should probably eventually be part of the styling too, but it's debug trees so they shouldn't be seen by the end user and only by me;
	protected virtual Style LabelStyle { get; } = new Style(Color.Gray35);
	protected virtual Style PunctuationStyle { get; } = new Style(Color.Gray23);
	protected virtual Style ElementIndexStyle { get; } = new Style(Color.Blue);
	protected virtual Style NullStyle { get; } = new Style(Color.Gray, decoration: Decoration.Italic);

	protected IClassificationStyling Styles { get; }
	#endregion

	#region Constructors
	public DefaultDebugTreePrinter(IClassificationStyling styles) => Styles = styles;
	#endregion

	#region Functions
	public static Tree Convert(IDebugTree tree, IClassificationStyling styles)
	{
		DefaultDebugTreePrinter printer = new(styles);
		return printer.Convert(tree);
	}
	public static Tree Convert(IDebugTreeObject obj, IClassificationStyling styles)
	{
		DefaultDebugTreePrinter printer = new(styles);
		return printer.Convert(obj);
	}
	public static void Print(IDebugTree tree, IClassificationStyling styles)
	{
		Tree t = Convert(tree, styles);
		AnsiConsole.Write(t);
	}
	public static void Print(IDebugTreeObject obj, IClassificationStyling styles)
	{
		Tree tree = Convert(obj, styles);
		AnsiConsole.Write(tree);
	}
	#endregion

	#region Methods
	public virtual Tree Convert(IDebugTree tree) => Convert("Tree", tree);
	public virtual Tree Convert(IDebugTreeObject obj) => Convert("Node", obj);

	protected virtual Tree Convert(string labelText, object? value)
	{
		Segment label = New(labelText, LabelStyle);
		return Convert(label, value);
	}
	protected virtual Tree Convert(Segment label, object? value)
	{
		if (value is IDebugTreeObject obj)
			return Convert(label, obj);

		if (value is IDebugTreeList list)
			return Convert(label, list);

		if (TryConvertCustom(label, value, out Tree? custom))
			return custom;

		Segments segments =
		[
			label,
			New(": ", PunctuationStyle),
			..ConvertSimple(value)
		];

		Markup markup = segments.ToMarkup();
		return new(markup);
	}
	protected virtual Tree Convert(Segment label, IDebugTreeObject node)
	{
		Tree tree = new(label.ToMarkup());

		foreach (IDebugTreeProperty property in node.Properties)
		{
			Tree branch = Convert(property.Label, property.Value);
			tree.AddNode(branch);
		}

		return tree;
	}
	protected virtual Tree Convert(Segment label, IDebugTreeList node)
	{
		Tree tree = new(label.ToMarkup());

		foreach (IDebugTreeListElement element in node.Elements)
		{
			Segment index = New($"#{element.Index}", ElementIndexStyle);
			Tree branch = Convert(index, element.Value);
			tree.AddNode(branch);
		}

		return tree;
	}

	protected virtual bool TryConvertCustom(Segment label, object? value, [NotNullWhen(true)] out Tree? branch)
	{
		branch = default;
		return false;
	}
	protected virtual Segments ConvertDebugText(IDebugTreeText text)
	{
		TextFragmentCollection fragments = text.Fragments;
		return fragments.Style(Styles);
	}
	protected virtual Segments ConvertSimple(object? value)
	{
		if (value is IDebugTreeText debugText)
			return ConvertDebugText(debugText);

		string? text = value?.ToString();

		Segment segment = New(
			text ?? "<null>",
			text is null ? NullStyle : Style.Plain);

		return [segment];
	}
	#endregion

	#region Helpers
	protected virtual Segment New(string text) => new(text, Style.Plain, null);
	protected virtual Segment New(string text, Style style) => new(text, style, null);
	protected virtual Segment New(string text, ClassificationKind? classification)
	{
		Style style = Styles.Get(classification).AsSpectre;
		return new(text, style, null);
	}
	#endregion
}
