using OwlDomain.ParsingTools.Text;

namespace OwlDomain.Owl.Code.CodeAnalysis.Parsing;

public sealed class LexingResult : SourceStageResult
{
	#region Properties
	public override string Stage => "lexing";
	public IReadOnlyList<ISyntaxToken> Tokens { get; }
	#endregion

	#region Constructors
	public LexingResult(IDiagnosticBag diagnostics, IPerformanceResult performance, ISourceFile source, IReadOnlyList<ISyntaxToken> tokens) : base(diagnostics, performance, source)
	{
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

			return new LexingResult(lexer.Diagnostics, performance, source, lexer.Tokens);
		}
	}
	#endregion

	#region Methods
	protected override bool LexTokens()
	{
		return
			TryLexSimpleToken("=>", SyntaxKind.EqualArrow) ||
			TryLexSimpleToken("==", SyntaxKind.DoubleEqualSign) ||
			TryLexSimpleToken("=", SyntaxKind.EqualSign) ||
			TryLexSimpleToken("{", SyntaxKind.OpenBrace) ||
			TryLexSimpleToken("}", SyntaxKind.CloseBrace) ||
			TryLexSimpleToken("(", SyntaxKind.OpenBracket) ||
			TryLexSimpleToken(")", SyntaxKind.CloseBracket) ||
			TryLexSimpleToken("[", SyntaxKind.OpenSquareBracket) ||
			TryLexSimpleToken("]", SyntaxKind.CloseSquareBracket) ||
			TryLexSimpleToken("<", SyntaxKind.OpenAngleBracket) ||
			TryLexSimpleToken(">", SyntaxKind.CloseAngleBracket) ||
			TryLexSimpleToken("+", SyntaxKind.Plus) ||
			TryLexSimpleToken("-", SyntaxKind.Minus) ||
			TryLexSimpleToken("*", SyntaxKind.Star) ||
			TryLexSimpleToken("/", SyntaxKind.Divide) ||
			TryLexSimpleToken(".", SyntaxKind.Period) ||
			TryLexSimpleToken("?", SyntaxKind.QuestionMark) ||
			TryLexSimpleToken(":", SyntaxKind.Colon) ||
			TryLexSimpleToken(";", SyntaxKind.Semicolon) ||
			TryLexIdentifierOrKeyword() ||
			TryLexString() ||
			TryLexInterpolatedString()
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

		IndexedLinePosition end = Text.Position;
		FinishFullToken(out TriviaList leading, out TriviaList trailing);

		if (Keywords.TryGetValue(lexeme, out SyntaxKind kind))
			lexeme = lexeme.TryIntern();
		else
			kind = SyntaxKind.Identifier;

		SyntaxToken token = new(kind, new(start, end), lexeme, lexeme, leading, trailing);
		Tokens.Add(token);

		return true;
	}
	#endregion

	#region String methods
	private bool TryLexString()
	{
		if (TryLexStringStart() is false)
			return false;

		while (Text.HasRemaining)
		{
			if (Text.Current.IsLineBreak)
				break;

			if (TryLexStringEnd())
				return true;

			if (TryLexStringEscape() || TryLexStringText(false, '"', '\\'))
				continue;
		}

		ReportUnclosedString(new(Text.Position, Text.Position));
		return true;
	}
	private bool TryLexInterpolatedString()
	{
		if (TryLexInterpolatedStringStart() is false)
			return false;

		while (Text.HasRemaining)
		{
			if (Text.Current.IsLineBreak)
				break;

			if (TryLexStringEnd())
				return true;

			if (TryLexStringInterpolation() || TryLexStringEscape() || TryLexStringText(false, '"', '\\', '{'))
				continue;
		}

		ReportUnclosedString(new(Text.Position, Text.Position));
		return true;
	}
	private bool TryLexStringStart()
	{
		IndexedLinePosition start = Text.Position;
		if (Text.Match('"') is false)
			return false;

		FinishPrefixToken(out TriviaList leadingTrivia);
		SyntaxToken token = new(SyntaxKind.StringStart, new(start, Text.Position), "\"", null, leadingTrivia, TriviaList.Empty);
		Tokens.Add(token);

		return true;
	}
	private bool TryLexInterpolatedStringStart()
	{
		IndexedLinePosition start = Text.Position;

		if ((Text.Current == '$' && Text.Next == '"') is false)
			return false;

		Text.Advance(2);

		FinishPrefixToken(out TriviaList leadingTrivia);
		SyntaxToken token = new(SyntaxKind.InterpolatedStringStart, new(start, Text.Position), "$\"", null, leadingTrivia, TriviaList.Empty);
		Tokens.Add(token);

		return true;
	}
	private bool TryLexStringEnd()
	{
		IndexedLinePosition start = Text.Position;
		if (Text.Match('"') is false)
			return false;

		FinishSuffixToken(out TriviaList trailingTrivia);
		SyntaxToken token = new(SyntaxKind.StringEnd, new(start, Text.Position), "\"", null, TriviaList.Empty, trailingTrivia);
		Tokens.Add(token);

		return true;
	}
	private bool TryLexStringEscape()
	{
		IndexedLinePosition start = Text.Position;
		if (Text.Match('\\') is false)
			return false;

		bool TryMatch(char ch, string value)
		{
			if (Text.Match(ch) is false)
				return false;

			FinishInfixToken();
			SyntaxToken token = new(SyntaxKind.StringEscape, new(start, Text.Position), $"\\{ch}", value);
			Tokens.Add(token);

			return true;
		}

		bool matched =
			TryMatch('n', "\n") ||
			TryMatch('r', "\r") ||
			TryMatch('t', "\t") ||
			TryMatch('f', "\f") ||
			TryMatch('v', "\v") ||
			TryMatch('b', "\b") ||
			TryMatch('a', "\a") ||
			TryMatch('e', "\e") ||
			TryMatch('"', "\"")
		;

		if (matched)
			return true;

		FinishInfixToken();
		SyntaxToken badToken = new(SyntaxKind.StringEscape, new(start, Text.Position), "\\", "\\");
		Tokens.Add(badToken);

		ReportUnknownEscapeSequence(new(start, Text.Position));
		return true;
	}
	private bool TryLexStringText(bool allowLineBreak, params ReadOnlySpan<char> stopAt)
	{
		bool IsAtStop(ReadOnlySpan<char> stopAt)
		{
			TextElement current = Text.Current;

			if (allowLineBreak is false && current.IsLineBreak)
				return true;

			foreach (char stop in stopAt)
			{
				if (current == stop)
					return true;
			}

			return false;
		}

		if (Text.IsAtEnd || IsAtStop(stopAt))
			return false;

		ThrowIfLexemeBuilderNotCleared();

		IndexedLinePosition start = Text.Position;

		while (Text.HasRemaining && (IsAtStop(stopAt) is false))
		{
			LexemeBuilder.Append(Text.Current.Value);
			Text.Advance();
		}

		string lexeme = GetLexeme();

		FinishInfixToken();
		SyntaxToken token = new(SyntaxKind.StringText, new(start, Text.Position), lexeme, lexeme);
		Tokens.Add(token);

		return true;
	}
	private bool TryLexStringInterpolation()
	{
		if (TryLexStringInterpolationStart() is false)
			return false;

		while (Text.HasRemaining)
		{
			if (Text.Current.IsLineBreak)
				break;

			if (TryLexStringInterpolationEnd())
				return true;

			LexTokenSequence();
		}

		ReportUnclosedStringInterpolation(new(Text.Position, Text.Position));
		return true;
	}
	private bool TryLexStringInterpolationStart()
	{
		IndexedLinePosition start = Text.Position;
		if (Text.Match('{') is false)
			return false;

		FinishSuffixToken(out TriviaList trailingTrivia);
		SyntaxToken token = new(SyntaxKind.StringInterpolationStart, new(start, Text.Position), "{", null, TriviaList.Empty, trailingTrivia);
		Tokens.Add(token);

		return true;
	}
	private bool TryLexStringInterpolationEnd()
	{
		IndexedLinePosition start = Text.Position;
		if (Text.Match('}') is false)
			return false;

		FinishPrefixToken(out TriviaList leadingTrivia);
		SyntaxToken token = new(SyntaxKind.StringInterpolationEnd, new(start, Text.Position), "}", null, leadingTrivia, TriviaList.Empty);
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
		AddError("bad_characters", badGroup.Position, "Unexpected characters found.");
	}
	private void ReportUnclosedString(IndexedPositionRange position)
	{
		AddError("unclosed_string", position, "Line breaks are not allowed in string fragments. Use a '\"' to close the string.");
	}
	private void ReportUnclosedStringInterpolation(IndexedPositionRange position)
	{
		AddError("unclosed_string_interpolation", position, "String value interpolation was not closed. Use a '}' to end the interpolation.");
	}
	private void ReportUnknownEscapeSequence(IndexedPositionRange position)
	{
		AddError("unknown_escape_sequence", position, "Unknown escape sequence.");
	}
	#endregion

	#region Helpers
	private void AddError(string id, IndexedPositionRange position, string message) => AddDiagnostic(DiagnosticKind.Error, id, position, message);
	private void AddDiagnostic(DiagnosticKind kind, string id, IndexedPositionRange position, string message)
	{
		Diagnostics.Add(new Diagnostic()
		{
			Provider = this,
			Kind = kind,
			Id = id,

			Location = new DiagnosticSourceLocation(Source, position),
			Message = message
		});
	}
	#endregion
}
