using System.Globalization;
using System.Text;

namespace OwlDomain.Owl.Code.CodeAnalysis.Parsing;

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
			TryLexSimpleToken("=>", SyntaxKind.EqualArrow) ||

			TryLexSimpleToken("==", SyntaxKind.DoubleEqualSign) ||
			TryLexSimpleToken("!=", SyntaxKind.NotEqual) ||
			TryLexSimpleToken("<=", SyntaxKind.LessThanOrEqual) ||
			TryLexSimpleToken(">=", SyntaxKind.GreaterThanOrEqual) ||

			TryLexSimpleToken("&&", SyntaxKind.DoubleAmpersand) ||
			TryLexSimpleToken("||", SyntaxKind.DoublePipe) ||

			TryLexSimpleToken("+=", SyntaxKind.PlusEqual) ||
			TryLexSimpleToken("-=", SyntaxKind.MinusEqual) ||
			TryLexSimpleToken("*=", SyntaxKind.StarEqual) ||
			TryLexSimpleToken("/=", SyntaxKind.DivideEqual) ||
			TryLexSimpleToken("%=", SyntaxKind.ModuloEqual) ||

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
			TryLexSimpleToken("%", SyntaxKind.Modulo) ||
			TryLexSimpleToken(".", SyntaxKind.Period) ||
			TryLexSimpleToken(",", SyntaxKind.Comma) ||
			TryLexSimpleToken("?", SyntaxKind.QuestionMark) ||
			TryLexSimpleToken(":", SyntaxKind.Colon) ||
			TryLexSimpleToken(";", SyntaxKind.Semicolon) ||
			TryLexIdentifierOrKeyword() ||
			TryLexString() ||
			TryLexNumber() ||
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

	#region Number methods
	private bool TryLexNumber()
	{
		TryLexNumberBase(out SyntaxToken? @base);
		TryLexInteger((@base?.Value as NumberBase?) ?? NumberBase.Decimal, out SyntaxToken? integer, allowDecimal: true);

		if (@base is null && integer is null)
			return false;

		if (@base is not null && integer is null)
		{
			// Error will be provided by the parser.
			return true;
		}

		if (@base is not null && integer is not null)
			return true;

		Debug.Assert(integer is not null);

		if (Text.Current == '.' && Rune.IsDigit(Text.Next.AsRune))
		{
			LexDecimalDot();
			TryLexInteger(NumberBase.Decimal, out _, allowDecimal: false);
		}

		return true;
	}
	private void LexDecimalDot()
	{
		Debug.Assert(Text.Current == '.');

		IndexedLinePosition start = Text.Position;
		Text.Advance();

		FinishInfixToken();
		SyntaxToken token = new(SyntaxKind.Period, new(start, Text.Position), ".", value: null);
		Tokens.Add(token);
	}
	private bool TryLexNumberBase([NotNullWhen(true)] out SyntaxToken? token)
	{
		IndexedLinePosition start = Text.Position;

		if (Text.MatchSequence("0x"))
		{
			FinishPrefixToken(out TriviaList leadingTrivia);
			token = new(SyntaxKind.IntegerBase, new(start, Text.Position), "0x", NumberBase.Hex, leadingTrivia, TriviaList.Empty);
			Tokens.Add(token);

			return true;
		}

		token = default;
		return false;
	}
	private bool TryLexInteger(NumberBase @base, [NotNullWhen(true)] out SyntaxToken? token, bool allowDecimal)
	{
		if (Rune.IsDigit(Text.Current.AsRune) is false)
		{
			token = default;
			return false;
		}

		ThrowIfLexemeBuilderNotCleared();
		ThrowIfValueBuilderNotCleared();

		IndexedLinePosition start = Text.Position;

		while (Text.HasRemaining)
		{
			if (Text.Match('_'))
			{
				LexemeBuilder.Append('_');
				continue;
			}

			TextElement current = Text.Current;
			if (Rune.IsDigit(current.AsRune))
			{
				LexemeBuilder.Append(current.Value);
				ValueBuilder.Append(current.Value);

				Text.Advance();
				continue;
			}

			break;
		}

		Debug.Assert(ValueBuilder.Length > 0);

		string lexeme = GetLexeme();
		string valueStr = GetValue();

		NumberStyles style = @base switch
		{
			NumberBase.Hex => NumberStyles.AllowHexSpecifier,
			NumberBase.Decimal => NumberStyles.Integer,

			_ or NumberBase.Unknown => ThrowHelper.ThrowInvalidOperationException<NumberStyles>($"Unknown number base ({@base}).")
		};

		object? value;

		if (style is NumberStyles.Integer)
		{
			if (long.TryParse(valueStr, style, provider: null, out long v) is false)
			{
				value = default;
				AddError("number_too_big", new(start, Text.Position), $"The number literal is too big.");
			}
			else
				value = v;
		}
		else
		{
			if (ulong.TryParse(valueStr, style, provider: null, out ulong v) is false)
			{
				value = default;
				AddError("invalid_base_number", new(start, Text.Position), "The base integer value is either too big, or contains invalid numbers, I'll add better checks later.");
			}
			else
				value = v;
		}

		IndexedLinePosition end = Text.Position;
		TriviaList leadingTrivia, trailingTrivia;

		if (allowDecimal && Text.Current == '.' && Rune.IsDigit(Text.Next.AsRune))
		{
			// Note(Nightowl): Not a problem if this is actually an infix because of the base, just a bit less efficient;
			FinishPrefixToken(out leadingTrivia);
			trailingTrivia = TriviaList.Empty;
		}
		else
		{
			// Note(Nightowl): Not a problem if this is actually a suffix because of the base, just a bit less efficient;
			FinishFullToken(out leadingTrivia, out trailingTrivia);
		}

		token = new(SyntaxKind.Integer, new(start, end), lexeme, value, leadingTrivia, trailingTrivia);
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
