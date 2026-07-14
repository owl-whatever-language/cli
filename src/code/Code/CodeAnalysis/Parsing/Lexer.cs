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
	#region Nested types
	private readonly struct StringErrorScope(Lexer lexer, bool set) : IDisposable
	{
		#region Methods
		public void Dispose()
		{
			if (set)
				lexer._stringErrors = null;
		}
		#endregion
	}
	#endregion

	#region Fields
	private int? _stringErrors;
	#endregion

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
		object? value = lexeme;

		IndexedLinePosition end = Text.Position;
		FinishFullToken(out TriviaList leading, out TriviaList trailing);

		if (Keywords.TryGetValue(lexeme, out SyntaxKind kind))
			lexeme = lexeme.TryIntern();
		else if (lexeme is "true")
		{
			kind = SyntaxKind.Boolean;
			value = true;
		}
		else if (lexeme is "false")
		{
			kind = SyntaxKind.Boolean;
			value = false;
		}
		else
			kind = SyntaxKind.Identifier;

		SyntaxToken token = new(kind, new(start, end), lexeme, value, leading, trailing);
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

		foreach (NumberBase @base in NumberBase.WithSpecifier)
		{
			Debug.Assert(@base.Specifier is not null);

			if (Text.MatchSequence(@base.Specifier) is false)
				continue;

			FinishPrefixToken(out TriviaList leadingTrivia);
			token = new(SyntaxKind.IntegerBase, new(start, Text.Position), @base.Specifier, @base, leadingTrivia, TriviaList.Empty);
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
			if (Rune.IsLetterOrDigit(current.AsRune))
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
		object? value;

		IndexedPositionRange position = new(start, Text.Position);

		NumberStyles style = @base.Kind switch
		{
			NumberBaseKind.Binary => NumberStyles.AllowBinarySpecifier,
			NumberBaseKind.Decimal => NumberStyles.Integer,
			NumberBaseKind.Hexadecimal => NumberStyles.AllowHexSpecifier,

			_ => ThrowHelper.ThrowInvalidOperationException<NumberStyles>($"Unknown number base kind ({@base.Kind}).")
		};

		if (style is NumberStyles.Integer)
			value = long.TryParse(valueStr, style, provider: null, out long v) ? v : default;
		else
			value = ulong.TryParse(valueStr, style, provider: null, out ulong v) ? v : default;

		if (value is null)
		{
			if (@base.IsValid(valueStr))
			{
				Diagnostics
					.BuildError(this, "number_too_big")
					.Add(Source, position, lines => lines.AddLine("The number had too many digits."));
			}
			else
			{
				Diagnostics
					.BuildError(this, "invalid_number_characters")
					.Add(Source, position, lines =>
					{
						if (@base.Kind == NumberBaseKind.Decimal)
							lines.AddLine("The number literal contained invalid characters.");
						else
							lines.AddLine("The number literal contained invalid characters for this base.");

						lines.AddLine(
							"Please only use the '",
							(@base.CharacterSetDisplay, ClassificationKind.Number),
							"' characters, or an underscore '",
							("_", ClassificationKind.Number),
							"' to separate the digits.");
					});
			}
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
		if (TryLexStringStart(out SyntaxToken? start) is false)
			return false;

		using (StringScope())
		{
			while (Text.HasRemaining)
			{
				if (Text.Current.IsLineBreak)
					break;

				if (TryLexStringEnd())
					return true;

				if (TryLexStringEscape() || TryLexStringText(false, '"', '\\'))
					continue;
			}

			if (_stringErrors is 0)
			{
				_stringErrors++;

				Diagnostics
					.BuildError(this, "unclosed_string")
					.Add(Source, Text.Position, lines => lines.AddLine("Line breaks are not allowed in string fragments. Use a quote mark '", ('"', ClassificationKind.String), "' to close the string."))
					.Add(start, lines => lines.AddLine("Need to match this quote mark '", ('"', ClassificationKind.String), "'."));
			}
		}

		return true;
	}
	private bool TryLexInterpolatedString()
	{
		if (TryLexInterpolatedStringStart(out SyntaxToken? start) is false)
			return false;

		using (StringScope())
		{
			while (Text.HasRemaining)
			{
				if (Text.Current.IsLineBreak)
					break;

				if (TryLexStringEnd())
					return true;

				if (TryLexStringInterpolation() || TryLexStringEscape() || TryLexStringText(false, '"', '\\', '{'))
					continue;
			}

			if (_stringErrors is 0)
			{
				_stringErrors++;
				Diagnostics
					.BuildError(this, "unclosed_string")
					.Add(Source, Tokens.Last().Position.End, lines => lines.AddLine("Line breaks are not allowed in string fragments. Use a quote mark '", ('"', ClassificationKind.String), "' to close the string."))
					.Add(start, lines => lines.AddLine("Need to match this quote mark '", ('"', ClassificationKind.String), "'."));
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TryLexStringStart() => TryLexStringStart(out _);
	private bool TryLexStringStart([NotNullWhen(true)] out SyntaxToken? token)
	{
		IndexedLinePosition start = Text.Position;
		if (Text.Match('"') is false)
		{
			token = default;
			return false;
		}

		FinishPrefixToken(out TriviaList leadingTrivia);
		token = new(SyntaxKind.StringStart, new(start, Text.Position), "\"", null, leadingTrivia, TriviaList.Empty);
		Tokens.Add(token);

		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TryLexInterpolatedStringStart() => TryLexInterpolatedStringStart(out _);
	private bool TryLexInterpolatedStringStart([NotNullWhen(true)] out SyntaxToken? token)
	{
		IndexedLinePosition start = Text.Position;

		if ((Text.Current == '$' && Text.Next == '"') is false)
		{
			token = default;
			return false;
		}

		Text.Advance(2);

		FinishPrefixToken(out TriviaList leadingTrivia);
		token = new(SyntaxKind.InterpolatedStringStart, new(start, Text.Position), "$\"", null, leadingTrivia, TriviaList.Empty);
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

		bool matched = false;
		foreach (EscapeSequence escape in EscapeSequence.Known)
		{
			if (TryMatch(escape.Character, escape.Value))
			{
				matched = true;
				break;
			}
		}

		if (matched)
			return true;

		FinishInfixToken();
		SyntaxToken badToken = new(SyntaxKind.StringEscape, new(start, Text.Position), "\\", "\\");
		Tokens.Add(badToken);

		Diagnostics
					.BuildError(this, "unknown_escape_sequence")
					.Add(badToken, lines =>
					{
						lines.AddLine("This escape sequence is not recognised.");
						lines.AddLine("The currently recognised escape sequences are:");
						foreach (EscapeSequence escape in EscapeSequence.Known)
							lines.AddLine($"• \\{escape.Character} - {escape.Name}.");
					});

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
		Debug.Assert(_stringErrors is not null);

		if (TryLexStringInterpolationStart(out SyntaxToken? start) is false)
			return false;

		while (Text.HasRemaining)
		{
			if (Text.Current.IsLineBreak)
				break;

			if (TryLexStringInterpolationEnd())
				return true;

			LexTokenSequence();
		}

		if (_stringErrors is 0)
		{
			_stringErrors++;
			Diagnostics
				.BuildError(this, "unclosed_string_interpolation")
				.Add(Source, Tokens.Last().Position.End, lines => lines.AddLine("String value interpolation was not closed. Use a closing brace '", ("}", ClassificationKind.Punctuation), "' to end the interpolation."))
				.Add(start, lines => lines.AddLine("Need to match this opening brace '", ("{", ClassificationKind.Punctuation), "'."));
		}

		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TryLexStringInterpolationStart() => TryLexStringInterpolationStart(out _);
	private bool TryLexStringInterpolationStart([NotNullWhen(true)] out SyntaxToken? token)
	{
		IndexedLinePosition start = Text.Position;
		if (Text.Match('{') is false)
		{
			token = default;
			return false;
		}

		FinishSuffixToken(out TriviaList trailingTrivia);
		token = new(SyntaxKind.StringInterpolationStart, new(start, Text.Position), "{", null, TriviaList.Empty, trailingTrivia);
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

	#region Helpers
	private StringErrorScope StringScope()
	{
		if (_stringErrors is null)
		{
			_stringErrors = 0;
			return new(this, true);
		}

		return new(this, false);
	}
	#endregion
}
