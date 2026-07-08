namespace OwlDomain.ParsingTools.Styling;

[Flags]
public enum StylingEffect
{
	None = 0,
	Bold = 1 << 0,
	Italic = 1 << 1,
	Underline = 1 << 2,

	/// <summary>This should be preferred over the underlined and dotted effects.</summary>
	Wavy = 1 << 3,

	/// <summary>This should be preferred over the underlined effect.</summary>
	Dotted = 1 << 4,

	Dim = 1 << 5,

	WavyOrUnderlined = Wavy | Underline,
	DottedOrUnderlined = Dotted | Underline,
	WavyOrDotted = Wavy | Dotted,
	WavyOrDottedOrUnderlined = Wavy | Dotted | Underline
}
