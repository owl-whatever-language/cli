using System.Drawing;

namespace OwlDomain.Owl.Code.Styling;

public static class OwlStyling
{
	#region Properties
	public static IClassificationStyling Default => DefaultDark;
	public static IClassificationStyling DefaultDark => field ??= CreateDefaultDark();
	#endregion

	#region Helpers
	private static IClassificationStyling CreateDefaultDark()
	{
		ClassificationStyling styling = new()
		{
			DefaultStyle = new("#d7d7d7"),
			Background = ColorTranslator.FromHtml("#171717"),
		};

		styling
			// Diagnostics
			.Add(ClassificationKind.Error, "#ff0000", StylingEffect.Italic | StylingEffect.Wavy)
			.Add(ClassificationKind.Warning, "#ffaf00", StylingEffect.Italic | StylingEffect.Wavy)
			.Add(ClassificationKind.Hint, "#808080", StylingEffect.Italic | StylingEffect.Dotted)

			// Pretty source
			.Add(ClassificationKind.PrettySource, "#808080")
			.Add(ClassificationKind.Margin, "#808080")
			.Add(ClassificationKind.LineNumber, "#3d8de9")

			// Trivia
			.Add(ClassificationKind.Comment, "#589058")

			// Punctuation
			.Add(ClassificationKind.Punctuation, "#808080")
			.Add(ClassificationKind.Operator, "#e0e0e0")

			// Literals
			.Add(ClassificationKind.String, "#d7af87")
			.Add(ClassificationKind.Number, "#b8dea4")

			// Names
			.Add(ClassificationKind.Identifier, "#b2b2b2")
			.Add(ClassificationKind.Keyword, "#3d8de9")
			.Add(ClassificationKind.Type, "#00af87")
			.Add(ClassificationKind.Variable, "#fefefe")
			.Add(ClassificationKind.Function, "#8054af")
			.Add(ClassificationKind.Parameter, "#fcffcc")
		;

		return styling;
	}
	#endregion
}
