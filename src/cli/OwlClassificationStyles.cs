using OwlDomain.ParsingTools.Classification;

namespace OwlDomain.Owl.CLI;

public class OwlClassificationStyles : ClassificationStyles
{
	#region Properties
	private static IReadOnlyDictionary<ClassificationKind, Style> Styles { get; } = new Dictionary<ClassificationKind, Style>
	{
		{ ClassificationKind.Comment, new(Color.DarkGreen) },
		{ ClassificationKind.Punctuation, new(Color.Gray50) },
		{ ClassificationKind.Keyword, new(Color.Blue)  },
		{ ClassificationKind.Type, new(Color.DarkCyan) },
		{ ClassificationKind.String, new(Color.Tan) },
		{ ClassificationKind.Function, new(Color.MediumPurple3) },
		{ ClassificationKind.Parameter, new(Color.Cornsilk1) },
		{ ClassificationKind.Variable, new(Color.White) },
		{ ClassificationKind.Identifier, new(Color.Gray70) },
	};

	public static OwlClassificationStyles Instance { get; } = new();
	#endregion

	#region Constructors
	private OwlClassificationStyles() : base(Styles) { }
	#endregion
}
