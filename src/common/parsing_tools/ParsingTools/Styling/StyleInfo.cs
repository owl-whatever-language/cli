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
