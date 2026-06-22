namespace OwlDomain.ParsingTools.Syntax;

using Segment = AnsiMarkupSegment;
using Segments = IReadOnlyList<AnsiMarkupSegment>;

public class DefaultDebugTreePrinter
{
	#region Properties
	protected virtual Style LabelStyle { get; } = new Style(Color.Gray35);
	protected virtual Style PunctuationStyle { get; } = new Style(Color.Gray23);
	protected virtual Style ElementIndexStyle { get; } = new Style(Color.Blue);
	protected virtual Style NullStyle { get; } = new Style(Color.Gray, decoration: Decoration.Italic);

	protected IClassificationStyles Styles { get; }
	#endregion

	#region Constructors
	public DefaultDebugTreePrinter(IClassificationStyles styles) => Styles = styles;
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
	protected virtual Segments ConvertSimple(object? value)
	{
		if (value is ClassificationKind classification)
			return ConvertClassification(classification);

		if (value is ISyntaxNode node)
			return ConvertSource(node);

		string? text = value?.ToString();

		Segment segment = New(
			text ?? "<null>",
			text is null ? NullStyle : Style.Plain);

		return [segment];
	}
	protected virtual Segments ConvertClassification(ClassificationKind classification)
	{
		List<Segment> segments = [];

		IReadOnlyList<ClassificationKind> parts = classification.Split();

		ClassificationKind? current = null;

		for (int i = 0; i < parts.Count; i++)
		{
			if (i > 0)
				segments.Add(new(".", PunctuationStyle, null));

			ClassificationKind part = parts[i];
			current = current is null ? part : current.Value + part.Name;

			segments.Add(New(part.Name, current));
		}

		return segments;
	}
	protected virtual Segments ConvertSource(ISyntaxNode node)
	{
		return node.ToTextFragments(false).Style(Styles);
	}
	#endregion

	#region Helpers
	protected virtual Segment New(string text) => new(text, Style.Plain, null);
	protected virtual Segment New(string text, Style style) => new(text, style, null);
	protected virtual Segment New(string text, ClassificationKind? classification)
	{
		Style style = Styles.GetStyle(classification);
		return new(text, style, null);
	}
	#endregion
}
