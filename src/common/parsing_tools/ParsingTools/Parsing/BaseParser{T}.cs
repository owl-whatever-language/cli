namespace OwlDomain.ParsingTools.Parsing;

public abstract class BaseParser<TToken> : BaseParser
	where TToken : notnull, ISyntaxToken
{
	#region Constructors
	protected BaseParser(ISourceFile source, IReadOnlyList<ISyntaxToken> tokens) : base(source, tokens) { }
	#endregion

	#region Match methods
	protected bool Match(SyntaxKind kind, [NotNullWhen(true)] out TToken? token)
	{
		if (Match(kind, out ISyntaxToken? untyped))
		{
			token = Convert(untyped);
			return true;
		}

		token = default;
		return false;
	}
	protected bool Match(SyntaxKind kind, ClassificationKind classification, [NotNullWhen(true)] out TToken? token)
	{
		if (Match(kind, out ISyntaxToken? untyped))
		{
			token = Convert(untyped, classification);
			return true;
		}

		token = default;
		return false;
	}
	protected bool MatchAny([NotNullWhen(true)] out TToken? token, ClassificationKind classification, params ReadOnlySpan<SyntaxKind> kinds)
	{
		if (MatchAny(out ISyntaxToken? untyped, kinds))
		{
			token = Convert(untyped, classification);
			return true;
		}

		token = default;
		return false;
	}
	protected bool MatchAny([NotNullWhen(true)] out TToken? token, ClassificationKind classification, params IReadOnlyCollection<SyntaxKind> kinds)
	{
		if (MatchAny(out ISyntaxToken? untyped, kinds))
		{
			token = Convert(untyped, classification);
			return true;
		}

		token = default;
		return false;
	}
	#endregion

	#region Expect methods
	protected TToken ExpectSilent(SyntaxKind kind)
	{
		ISyntaxToken token = ExpectSilentCore(kind);
		return Convert(token);
	}
	protected TToken ExpectSilent(SyntaxKind kind, ClassificationKind classification)
	{
		ISyntaxToken token = ExpectSilentCore(kind);
		return Convert(token, classification);
	}

	protected TToken Expect(SyntaxKind kind, ClassificationKind classification, string lexeme, string purpose)
	{
		return Expect(kind, classification, lexeme, purpose, out _);
	}
	protected TToken Expect(SyntaxKind kind, ClassificationKind classification, string lexeme, string purpose, out Diagnostic? diagnostic)
	{
		if (Match(kind, classification, out TToken? token))
		{
			diagnostic = default;
			return token;
		}

		token = Fabricate(kind, classification);
		diagnostic = ReportExpected(token, lexeme, purpose);

		return token;
	}

	protected TToken Expect(SyntaxKind kind, ClassificationKind classification, string message)
	{
		return Expect(kind, classification, message, out _);
	}
	protected TToken Expect(SyntaxKind kind, ClassificationKind classification, string message, out Diagnostic? diagnostic)
	{
		if (Match(kind, classification, out TToken? token))
		{
			diagnostic = default;
			return token;
		}

		token = Fabricate(kind, classification);
		diagnostic = ReportExpected(token, message);

		return token;
	}

	protected TToken Expect(SyntaxKind kind, ClassificationKind classification, Action<TextFragmentLineCollection> message)
	{
		return Expect(kind, classification, message, out _);
	}
	protected TToken Expect(SyntaxKind kind, ClassificationKind classification, Action<TextFragmentLineCollection> message, out Diagnostic? diagnostic)
	{
		if (Match(kind, classification, out TToken? token))
		{
			diagnostic = default;
			return token;
		}

		token = Fabricate(kind, classification);
		diagnostic = ReportExpected(token, message);

		return token;
	}

	protected TToken ExpectClosing(
		ISyntaxToken opening,
		SyntaxKind kind,
		ClassificationKind classification,
		string closingLexeme,
		string purpose)
	{
		return ExpectClosing(opening, kind, classification, closingLexeme, purpose, out _);
	}
	protected TToken ExpectClosing(
		ISyntaxToken opening,
		SyntaxKind kind,
		ClassificationKind classification,
		string closingLexeme,
		string purpose,
		out Diagnostic? diagnostic)
	{
		if (Match(kind, classification, out TToken? token))
		{
			diagnostic = default;
			return token;
		}

		token = Fabricate(kind, classification);
		diagnostic = ReportExpectedClosing(opening, token, closingLexeme, purpose);

		return token;
	}

	protected TToken ExpectEndOfInput() => ExpectEndOfInput(out _);
	protected TToken ExpectEndOfInput(out Diagnostic? diagnostic)
	{
		if (Match(SyntaxKind.EndOfInput, out TToken? token))
		{
			diagnostic = default;
			return token;
		}

		token = Fabricate(SyntaxKind.EndOfInput);
		diagnostic = ReportExpectedEndOfInput(token);

		return token;
	}
	#endregion

	#region Fabricate methods
	protected TToken Fabricate(SyntaxKind kind)
	{
		ISyntaxToken token = FabricateCore(kind);
		return Convert(token);
	}
	protected TToken Fabricate(SyntaxKind kind, ClassificationKind classification)
	{
		ISyntaxToken token = FabricateCore(kind);
		return Convert(token, classification);
	}
	#endregion

	#region Convert methods
	[return: NotNullIfNotNull(nameof(token))]
	protected abstract TToken? Convert(ISyntaxToken? token, ClassificationKind? classification = null);
	#endregion

	#region Recovery methods
	protected sealed override ISyntaxNode? TryParseBadSyntax()
	{
		ISyntaxNode? attempt = TryParseBadSyntaxCore();
		if (attempt is not null)
			return attempt;

		ISyntaxToken? current = Current;
		if (current is not null)
			Advance();

		return Convert(current);
	}
	protected abstract ISyntaxNode? TryParseBadSyntaxCore();

	protected void SkipCurrent(ClassificationKind? classification = null)
	{
		ISyntaxToken? badSyntax = Convert(Current, classification);
		if (badSyntax is null)
			return;

		Advance();
		ToBadSyntaxTrivia(badSyntax);
	}
	#endregion
}
