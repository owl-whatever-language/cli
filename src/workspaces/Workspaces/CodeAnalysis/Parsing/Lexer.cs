namespace OwlDomain.Owl.Workspaces.CodeAnalysis.Parsing;

public sealed class LexingResult : ISourceStageResult, IStageResultDiagnostics, IStageResultPerformance
{
	#region Properties
	public string Stage => "lexing";
	public ISourceFile Source { get; }
	public IDiagnosticBag Diagnostics { get; }
	public IPerformanceResult Performance { get; }
	public IReadOnlyList<ISyntaxToken> Tokens { get; }
	#endregion

	#region Constructors
	public LexingResult(
		ISourceFile source,
		IDiagnosticBag diagnostics,
		IPerformanceResult performance,
		IReadOnlyList<ISyntaxToken> tokens)
	{
		Source = source;
		Diagnostics = diagnostics;
		Performance = performance;
		Tokens = tokens;
	}
	#endregion
}

public sealed class Lexer : BaseLexer, IDiagnosticProvider
{
	#region Properties
	private static IReadOnlyDictionary<string, SyntaxKind> Keywords { get; } = SyntaxKind.AllKeywords.ToDictionary(s => s.Name);
	public string Name => "lexer";
	private ISourceFile Source { get; }
	private DiagnosticBag Diagnostics { get; } = [];
	#endregion

	#region Constructors
	private Lexer(ISourceFile source, ITextParser text) : base(text)
	{
		Source = source;
	}
	#endregion

	#region Functions
	public static LexingResult Lex(ISourceFile source)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			ITextParser text = source.CreateParser();
			Lexer lexer = new(source, text);

			lexer.Lex();

			return new LexingResult(source, lexer.Diagnostics, performance, lexer.Tokens);
		}
	}
	#endregion

	#region Methods
	protected override bool LexTokens()
	{
		return
			TryLexSimpleToken("{", SyntaxKind.OpenBrace) ||
			TryLexSimpleToken("}", SyntaxKind.CloseBrace) ||

			TryLexSimpleToken(":", SyntaxKind.Colon) ||
			TryLexSimpleToken(";", SyntaxKind.Semicolon) ||
			TryLexIdentifierOrKeyword()
		;
	}
	private bool TryLexIdentifierOrKeyword()
	{
		ThrowIfLexemeBuilderNotCleared();

		if ((char.IsAsciiLetter(Text.Current.AsChar) || Text.Current == '_') is false)
			return false;

		IndexedLinePosition start = Text.Position;

		LexemeBuilder.Append(Text.Current.Value);
		Text.Advance();

		while (Text.HasRemaining && (Text.Current == '_' || char.IsAsciiLetterOrDigit(Text.Current.AsChar)))
		{
			LexemeBuilder.Append(Text.Current.Value);
			Text.Advance();
		}

		string lexeme = GetLexeme();
		object? value = lexeme;

		IndexedLinePosition end = Text.Position;
		FinishFullToken(out TriviaList leading, out TriviaList trailing);

		if (Keywords.TryGetValue(lexeme, out SyntaxKind kind))
			lexeme = lexeme.TryIntern();
		else
			kind = SyntaxKind.Identifier;

		SyntaxToken token = new(kind, new(start, end), lexeme, value, leading, trailing);
		Tokens.Add(token);

		return true;
	}
	#endregion

	#region Trivia methods
	protected override ISyntaxTrivia? LexTrivia()
	{
		return
			base.LexTrivia() ??
			TryLexComment();
	}
	private ISyntaxTrivia? TryLexComment()
	{
		ThrowIfLexemeBuilderNotCleared();
		ThrowIfValueBuilderNotCleared();

		IndexedLinePosition start = Text.Position;
		if (Text.MatchSequence("//") is false)
			return null;

		LexemeBuilder.Append("//");

		while (Text.HasRemaining && (Text.Current.IsLineBreak is false))
		{
			TextElement current = Text.Current;

			LexemeBuilder.Append(current.Value);
			ValueBuilder.Append(current.Value);

			Text.Advance();
		}

		string lexeme = GetLexeme();
		string value = GetValue().Trim();

		return new SyntaxTrivia(SyntaxKind.Comment, new(start, Text.Position), lexeme, ClassificationKind.SinglelineComment, value);
	}
	#endregion

	#region Diagnostic methods
	protected override void ReportBadCharacters(ISyntaxTrivia badGroup)
	{
		Debug.Assert(badGroup.Position.Length > 0);

		if (badGroup.Position.Length is 1)
		{
			Diagnostics
				.BuildError(this, "bad_character")
				.Add(badGroup, lines => lines.AddLine("This character is not recognised by the lexer."));
		}
		else
		{
			Diagnostics
				.BuildError(this, "bad_characters")
				.Add(badGroup, lines => lines.AddLine("These characters are not recognised by the lexer."));
		}
	}
	protected override void ReportTabAsAlignment(ISyntaxTrivia tab)
	{
		Diagnostics
			.BuildWarning(this, "tab_as_alignment")
			.Add(tab, lines =>
			{
				lines.AddLine("Tabs should only be used at the very start of the line for indentation. They should never be used for alignment.");
				lines.AddLine("This is not an OWL limitation, but a common problem with how tabs work in general.");
			});
	}
	#endregion
}
