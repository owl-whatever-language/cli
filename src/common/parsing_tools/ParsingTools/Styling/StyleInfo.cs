using System.Drawing;

namespace OwlDomain.ParsingTools.Styling;

public readonly struct StyleInfo :
#if NET7_0_OR_GREATER
	IEqualityOperators<StyleInfo, StyleInfo, bool>,
#endif
	IEquatable<StyleInfo>
{
	#region Properties
	public Color? Color { get; }
	public StylingEffect? Effect { get; }
	#endregion

	#region Constructors
	public StyleInfo(Color? color = null, StylingEffect? effect = null)
	{
		Color = color;
		Effect = effect;
	}
	public StyleInfo(string colorHex, StylingEffect? effect = null)
	{
		Color = ColorTranslator.FromHtml(colorHex);
		Effect = effect;
	}
	#endregion

	#region Functions
	public static StyleInfo Merge(params IReadOnlyList<StyleInfo> styles)
	{
		if (styles.Count is 0)
			return default;

		Color? color = styles[0].Color;
		StylingEffect? effect = styles[0].Effect;

		foreach (StyleInfo style in styles.Skip(1))
		{
			color ??= style.Color;

			if (style.Effect is not null)
			{
				effect ??= StylingEffect.None;
				effect |= style.Effect;
			}
		}

		return new(color, effect);
	}
	public static StyleInfo Merge(params ReadOnlySpan<StyleInfo> styles)
	{
		if (styles.Length is 0)
			return default;

		Color? color = styles[0].Color;
		StylingEffect? effect = styles[0].Effect;

		for (int i = 1; i < styles.Length; i++)
		{
			StyleInfo style = styles[i];
			color ??= style.Color;

			if (style.Effect is not null)
			{
				effect ??= StylingEffect.None;
				effect |= style.Effect;
			}
		}

		return new(color, effect);
	}
	#endregion

	#region Methods
	public bool Equals(StyleInfo other)
	{
		return
			Color == other.Color &&
			Effect == other.Effect;
	}
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		if (obj is StyleInfo other)
			return Equals(other);

		return false;
	}
	public override int GetHashCode() => HashCode.Combine(Color, Effect);
	#endregion

	#region Operators
	public static bool operator ==(StyleInfo left, StyleInfo right) => left.Equals(right);
	public static bool operator !=(StyleInfo left, StyleInfo right) => left.Equals(right) is false;
	#endregion
}
