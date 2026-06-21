namespace OwlDomain.ParsingTools.Classification;

public interface IClassificationStyles
{
	#region Methods
	bool TryGetStyle([NotNullWhen(true)] ClassificationKind? classification, out Style style);
	Style GetStyle(ClassificationKind? classification);
	#endregion
}

public class ClassificationStyles : IClassificationStyles
{
	#region Fields
	private readonly IReadOnlyDictionary<ClassificationKind, Style> _styles;
	#endregion

	#region Constructors
	public ClassificationStyles(IReadOnlyDictionary<ClassificationKind, Style> styles) => _styles = styles;
	#endregion

	#region Methods
	public bool TryGetStyle([NotNullWhen(true)] ClassificationKind? classification, out Style style)
	{
		if (classification is null)
		{
			style = default;
			return false;
		}

		foreach (ClassificationKind kind in classification.Value.Iterate())
		{
			if (_styles.TryGetValue(kind, out style))
				return true;
		}

		style = default;
		return false;
	}
	public Style GetStyle(ClassificationKind? classification)
	{
		if (TryGetStyle(classification, out Style style))
			return style;

		return Style.Plain;
	}
	#endregion
}
