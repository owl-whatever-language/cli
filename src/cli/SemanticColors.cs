namespace OwlDomain.Owl.CLI;

public class SemanticColors
{
	#region Properties
	public static Style ErrorStyle { get; } = new(Color.Red);
	public static Style WarningStyle { get; } = new(Color.Orange1);
	public static Style SuggestionStyle { get; } = new(Color.LightSkyBlue1);
	public static IReadOnlyDictionary<DiagnosticKind, Style> DiagnosticStyles { get; } = new Dictionary<DiagnosticKind, Style>()
	{
		{ DiagnosticKind.Error, ErrorStyle },
		{ DiagnosticKind.Warning, WarningStyle },
		{ DiagnosticKind.Suggestion, SuggestionStyle },
	};

	public static Style LineNumberStyle { get; } = new(Color.Gray);
	public static Style CommentStyle { get; } = new(Color.DarkGreen);
	public static Style TypeStyle { get; } = new(Color.DarkCyan);
	public static Style FunctionStyle { get; } = new(Color.Purple);
	public static Style LocalVariableStyle { get; } = new(Color.LightGoldenrod2);
	public static Style StringLiteralStyle { get; } = new(Color.Salmon1);
	public static Style NumberLiteralStyle { get; } = new(Color.Honeydew2);
	public static IReadOnlyDictionary<ClassificationKind, Style> ClassificationStyles { get; } = new Dictionary<ClassificationKind, Style>()
	{
		{ ClassificationKind.Function, FunctionStyle },
		{ ClassificationKind.Type, TypeStyle },
		{ ClassificationKind.Variable, LocalVariableStyle },
		{ ClassificationKind.String, StringLiteralStyle },
		{ ClassificationKind.Number, NumberLiteralStyle },
	};
	#endregion

	#region Functions
	public static Style GetStyle(DiagnosticKind kind)
	{
		if (DiagnosticStyles.TryGetValue(kind, out Style style))
			return style;

		return Style.Plain;
	}
	public static Style GetStyle(ITriviaNode trivia)
	{
		if (trivia.Kind == SyntaxKind.Comment)
			return CommentStyle;

		return Style.Plain;
	}
	public static Style GetStyle(IConcreteSyntaxToken token)
	{
		if (token.Classification is null)
			return Style.Plain;

		foreach (ClassificationKind kind in token.Classification.Value.Iterate())
		{
			if (ClassificationStyles.TryGetValue(kind, out Style style))
				return style;
		}

		return Style.Plain;
	}
	#endregion
}
