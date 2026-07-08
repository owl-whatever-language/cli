using System.Drawing;

namespace OwlDomain.ParsingTools.Classification;

public interface IClassificationStyling
{
	#region Properties
	Color? Background { get; }
	StyleInfo DefaultStyle { get; }
	#endregion

	#region Methods
	bool TryGet(ClassificationKind? classification, out StyleInfo style);
	StyleInfo Get(ClassificationKind? classification);
	StyleInfo Get(params IReadOnlyList<ClassificationKind> classifications);
	#endregion
}

public sealed class ClassificationStyling : IClassificationStyling
{
	#region Fields
	private readonly Dictionary<ClassificationKind, StyleInfo> _styles = [];
	#endregion

	#region Properties
	public Color? Background
	{
		get;
		set
		{
			if (field is not null)
				ThrowHelper.ThrowInvalidOperationException("The background color has already been set.");

			field = value;
		}
	}
	public StyleInfo DefaultStyle
	{
		get;
		set
		{
			if (field != default)
				ThrowHelper.ThrowInvalidOperationException("The default style has already been set.");

			field = value;
		}
	}
	#endregion

	#region Methods
	public ClassificationStyling Add(ClassificationKind classification, StylingEffect effect)
	{
		StyleInfo style = new(effect: effect);
		return Add(classification, style);
	}
	public ClassificationStyling Add(ClassificationKind classification, string colorHex, StylingEffect? effect = null)
	{
		StyleInfo style = new(colorHex, effect);
		return Add(classification, style);
	}
	public ClassificationStyling Add(ClassificationKind classification, StyleInfo style)
	{
		_styles.Add(classification, style);

		return this;
	}

	public bool TryGet(ClassificationKind? classification, out StyleInfo style)
	{
		if (classification is not null)
		{
			foreach (ClassificationKind kind in classification.Value.Iterate())
			{
				if (_styles.TryGetValue(kind, out style))
					return true;
			}
		}

		if (DefaultStyle != default)
		{
			style = DefaultStyle;
			return true;
		}

		style = default;
		return false;
	}
	public StyleInfo Get(ClassificationKind? classification)
	{
		if (TryGet(classification, out StyleInfo style))
			return style;

		return default;
	}
	public StyleInfo Get(IReadOnlyList<ClassificationKind> classifications)
	{
		Span<StyleInfo> styles = new StyleInfo[classifications.Count];

		// Note(Nightowl):
		// There might be some problems here if the first classification(s) result in a default style, but the rest do not.
		// That might also end up actually being the indented behaviour, not sure yet.

		for (int i = 0; i < classifications.Count; i++)
			styles[i] = Get(classifications[i]);

		return StyleInfo.Merge(styles);
	}
	#endregion
}
