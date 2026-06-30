namespace OwlDomain.Owl.CLI;

public class OwlClassificationStyles : ClassificationStyles
{
	#region Properties
	private static IReadOnlyDictionary<ClassificationKind, Style> Styles { get; } = new Dictionary<ClassificationKind, Style>
	{
		{ ClassificationKind.Comment, new(new(88,144,88)) }, // dark green
		{ ClassificationKind.Punctuation, new(Color.Gray50) },
		{ ClassificationKind.Keyword, new(new(61, 141, 233)) }, // blue
		{ ClassificationKind.Type, new(Color.DarkCyan) },
		{ ClassificationKind.String, new(Color.Tan) },
		{ ClassificationKind.Function, new(Color.MediumPurple3) },
		{ ClassificationKind.Parameter, new(Color.Cornsilk1) },
		{ ClassificationKind.Variable, new(Color.White) },
		{ ClassificationKind.Identifier, new(Color.Gray70) },

		{ ClassificationKind.Error, new(Color.Red, decoration: Decoration.Italic) },
		{ ClassificationKind.Warning, new(Color.Orange1, decoration: Decoration.Italic) },
		{ ClassificationKind.Hint, new(Color.Gray, decoration: Decoration.Italic) },

		{ ClassificationKind.PrettySource, new(Color.Gray) },
		{ ClassificationKind.Margin, new(Color.Gray) },
		{ ClassificationKind.LineNumber, new(new(61, 141, 233)) }, // blue
	};

	public static OwlClassificationStyles Instance { get; } = new();
	#endregion

	#region Constructors
	private OwlClassificationStyles() : base(Styles) { }
	#endregion
}
