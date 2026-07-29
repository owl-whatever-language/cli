namespace OwlDomain.ParsingTools.Parsing;

public abstract class BaseParser : IDiagnosticProvider
{
	#region Nested types
	/// <summary>
	/// 	Represents a scope that's used for guarding against infinite loops that can occur from fabricating tokens.
	/// </summary>
	/// <param name="parser">The parser instance.</param>
	/// <param name="token">The token that the parser is on at the start of the loop iteration.</param>
	protected readonly struct LoopGuardScope(BaseParser parser, ISyntaxToken token) : IDisposable
	{
		#region Properties
		private BaseParser Parser { get; } = parser;
		private ISyntaxToken Token { get; } = token;
		private int OldDiagnosticCount { get; } = parser.Diagnostics.Count;
		#endregion

		#region Methods
		/// <inheritdoc/>
		public void Dispose()
		{
			if (Parser.Current == Token)
			{
				if (OldDiagnosticCount >= Parser.Diagnostics.Count)
					Parser.ReportInfiniteLoop(Token);

				Parser.RecoverFromCurrent();
			}
		}
		#endregion
	}
	#endregion

	#region Fields
	private int _index;
	private readonly List<ISyntaxToken> _tokens;
	#endregion

	#region Properties
	public virtual string Name => "parser";
	protected ISourceFile Source { get; }
	protected DiagnosticBag Diagnostics { get; } = [];

	/// <summary>The tokens that should be parsed.</summary>
	/// <remarks>The parser might mutate some of the tokens for error recovery purposes.</remarks>
	protected IReadOnlyList<ISyntaxToken> Tokens => _tokens;

	protected ISyntaxToken? Previous => Peek(-1);

	/// <summary>The current token.</summary>
	protected ISyntaxToken? Current => Peek(0);

	/// <summary>The next token.</summary>
	protected ISyntaxToken? Next => Peek(1);

	/// <summary>Whether the parser went past the last token.</summary>
	[MemberNotNullWhen(false, nameof(Current))]
	protected bool IsAtEnd => _index >= Tokens.Count;

	/// <summary>Whether the parser went past the last token, or the current token is the end of input token.</summary>
	[MemberNotNullWhen(false, nameof(Current))]
	protected bool RealisticIsAtEnd => IsAtEnd || Current.Kind == SyntaxKind.EndOfInput;

	/// <summary>Whether the parser has tokens remaining to be parsed.</summary>
	[MemberNotNullWhen(true, nameof(Current))]
	protected bool HasRemaining => _index < Tokens.Count;

	/// <summary>Whether the parser has tokens remaining to be parsed, and the current token is not the end of input token.</summary>
	[MemberNotNullWhen(true, nameof(Current))]
	protected bool RealisticHasRemaining => HasRemaining && Current.Kind != SyntaxKind.EndOfInput;
	#endregion

	#region Constructors
	protected BaseParser(ISourceFile source, IReadOnlyList<ISyntaxToken> tokens)
	{
		Source = source;

		if (tokens.LastOrDefault()?.Kind != SyntaxKind.EndOfInput)
			ThrowHelper.ThrowArgumentException(nameof(tokens), $"Expected to have a {nameof(SyntaxKind.EndOfInput)} token as the last token.");

		_tokens = [.. tokens];
	}
	#endregion

	#region Parsing helpers
	[MemberNotNullWhen(true, nameof(Current))]
	protected bool IsCurrentAny(params ReadOnlySpan<SyntaxKind> kinds)
	{
		if (Current is null)
			return false;

		SyntaxKind current = Current.Kind;
		foreach (SyntaxKind kind in kinds)
		{
			if (current == kind)
				return true;
		}

		return false;
	}

	/// <summary>Gets the token at the given <paramref name="offset"/> from the current position.</summary>
	/// <param name="offset">The offset in terms of tokens.</param>
	/// <returns>The token at the given offset, or <see langword="null"/> if the end of the input was reached.</returns>
	protected ISyntaxToken? Peek(int offset)
	{
		if (IsAtEnd)
			return default;

		int index = _index + offset;
		if (index >= 0 && index < Tokens.Count)
			return Tokens[index];

		return null;
	}

	/// <summary>Advances the parser to the next position.</summary>
	/// <param name="amount">The amount of tokens to advance the position by.</param>
	/// <returns><see langword="true"/> if the position was moved, <see langword="false"/> if the end was already reached.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="amount"/> is less than or equal to one.</exception>
	protected bool Advance(int amount = 1)
	{
		Guard.IsGreaterThan(amount, 0);

		if (IsAtEnd)
			return false;

		_index += amount;
		return true;
	}

	protected bool Match(SyntaxKind kind, [NotNullWhen(true)] out ISyntaxToken? token)
	{
		if (Current?.Kind == kind)
		{
			token = Current;
			return Advance();
		}

		token = default;
		return false;
	}
	protected bool MatchAny([NotNullWhen(true)] out ISyntaxToken? token, params ReadOnlySpan<SyntaxKind> kinds)
	{
		ISyntaxToken? current = Current;

		if (current is not null)
		{
			foreach (SyntaxKind kind in kinds)
			{
				if (current.Kind == kind)
				{
					token = current;

					Advance();
					return true;
				}
			}
		}

		token = default;
		return false;
	}
	protected bool MatchAny([NotNullWhen(true)] out ISyntaxToken? token, params IReadOnlyCollection<SyntaxKind> kinds)
	{
		ISyntaxToken? current = Current;

		if (current is not null)
		{
			foreach (SyntaxKind kind in kinds)
			{
				if (current.Kind == kind)
				{
					token = current;

					Advance();
					return true;
				}
			}
		}

		token = default;
		return false;
	}
	protected ISyntaxToken ExpectCore(SyntaxKind kind, Action<ISyntaxToken> callback)
	{
		if (Match(kind, out ISyntaxToken? token))
			return token;

		token = FabricateCore(kind, null);
		callback.Invoke(token);

		return token;
	}
	protected ISyntaxToken ExpectSilentCore(SyntaxKind kind)
	{
		if (Match(kind, out ISyntaxToken? token))
			return token;

		return FabricateCore(kind, null);
	}
	protected SyntaxToken FabricateCore(SyntaxKind kind, object? value)
	{
		Debug.Assert(Tokens.Count > 0, "Must have at least one token representing the end of the input.");
		ISyntaxNode expected = Current ?? Tokens.Last();

		IndexedPositionRange position = new(expected.FullPosition.Start, expected.FullPosition.Start);
		return new(kind, position, value);
	}

	protected void RecoverUntilEndOfInput()
	{
		while (RealisticHasRemaining && Current.Kind != SyntaxKind.EndOfInput)
			RecoverFromCurrent();

		Debug.Assert(Current?.Kind == SyntaxKind.EndOfInput);
	}
	protected void RecoverFromCurrent()
	{
		if (Current is null || Current.Kind == SyntaxKind.EndOfInput)
			return;

		if (Next is null)
			ThrowHelper.ThrowInvalidOperationException("The very last token (which should be the special end of input token) cannot be skipped as it is required for error recovery.");

		bool hadCurrent = Current is not null;
		ISyntaxNode? badSyntax = TryParseBadSyntax();

		if (badSyntax is null && hadCurrent)
			ThrowHelper.ThrowInvalidOperationException($"{nameof(TryParseBadSyntax)}() can only return null if there's no current token.");

		if (badSyntax is null)
		{
			Debug.Assert(IsAtEnd);
			return;
		}

		ToBadSyntaxTrivia(badSyntax);
	}
	protected void ToBadSyntaxTrivia(ISyntaxNode badSyntax)
	{
		if (Current is null)
			ThrowHelper.ThrowInvalidOperationException($"{nameof(TryParseBadSyntax)}() shouldn't consume the very last token (which should be the special end of input token).");

		BadSyntaxTrivia newTrivia = new(badSyntax);
		TriviaList newList = new([newTrivia, .. Current.LeadingTrivia]);

		SyntaxToken typed = (SyntaxToken)Current;
		_tokens[_index] = typed.ReplaceLeadingTrivia(newList);
	}
	protected abstract ISyntaxNode? TryParseBadSyntax();

	/// <summary>Enters a scope for guarding against infinite loops that can occur during parsing from fabricating tokens.</summary>
	/// <returns>A scope to use for the loop guard.</returns>
	protected LoopGuardScope LoopGuard()
	{
		if (Current is null)
			ThrowHelper.ThrowInvalidDataException("Expected the current token to be available.");

		return new(this, Current);
	}
	#endregion

	#region Diagnostic methods
	protected virtual Diagnostic ReportInfiniteLoop(ISyntaxToken token) => Diagnostics.ReportInfiniteLoop(this, token);
	protected virtual Diagnostic ReportDuplicate(ISyntaxToken token) => Diagnostics.ReportDuplicate(this, token);
	protected virtual Diagnostic ReportExpected(ISyntaxToken token, string lexeme, string purpose)
	{
		return Diagnostics.ReportExpected(this, token, lexeme, purpose);
	}
	protected virtual Diagnostic ReportExpected(ISyntaxToken token, string message)
	{
		return Diagnostics.ReportExpected(this, token, message);
	}
	protected virtual Diagnostic ReportExpected(ISyntaxToken token, Action<TextFragmentLineCollection> message)
	{
		return Diagnostics.ReportExpected(this, token, message);
	}
	protected virtual Diagnostic ReportExpectedClosing(
			ISyntaxToken openingToken,
			ISyntaxToken closingToken,
			string closingLexeme,
			string purpose)
	{
		return Diagnostics.ReportExpectedClosing(this, openingToken, closingToken, closingLexeme, purpose);
	}
	protected virtual Diagnostic ReportExpectedEndOfInput(ISyntaxToken token) => Diagnostics.ReportExpectedEndOfInput(this, token);
	protected virtual Diagnostic ReportExpectedSimple(ISyntaxToken fabricatedToken, string kind, params IEnumerable<object?> message)
	{
		return Diagnostics
			.BuildError(this, $"expected_{kind}")
			.Add(fabricatedToken, lines => lines.AddLine(message));
	}
	#endregion
}
