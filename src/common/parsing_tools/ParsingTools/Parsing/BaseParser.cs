namespace OwlDomain.ParsingTools.Parsing;

public abstract class BaseParser
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
		private int OldDiagnosticCount { get; } = parser.DiagnosticCount;
		#endregion

		#region Methods
		/// <inheritdoc/>
		public void Dispose()
		{
			if (Parser.Current == Token)
			{
				if (OldDiagnosticCount >= Parser.DiagnosticCount)
					Parser.ReportInfiniteLoop(Token.Position);

				Parser.SkipCurrent();
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
	protected abstract int DiagnosticCount { get; }

	/// <summary>The tokens that should be parsed.</summary>
	/// <remarks>The parser might mutate some of the tokens for error recovery purposes.</remarks>
	protected IReadOnlyList<ISyntaxToken> Tokens => _tokens;

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
	protected BaseParser(IReadOnlyList<ISyntaxToken> tokens)
	{
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
	/// <exception cref="ArgumentOutOfRangeException">Thrown if the given <paramref name="offset"/> is less than zero.</exception>
	protected ISyntaxToken? Peek(int offset)
	{
		Guard.IsGreaterThanOrEqualTo(offset, 0);

		if (IsAtEnd)
			return default;

		int index = _index + offset;
		if (index < Tokens.Count)
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
					return true;
				}
			}
		}

		token = default;
		return false;
	}
	protected ISyntaxToken ExpectCore(SyntaxKind kind, string errorMessage)
	{
		if (Match(kind, out ISyntaxToken? token))
			return token;

		token = FabricateCore(kind);
		ReportExpectedToken(token.Position, kind, errorMessage);

		return token;
	}
	protected ISyntaxToken ExpectSilentCore(SyntaxKind kind)
	{
		if (Match(kind, out ISyntaxToken? token))
			return token;

		return FabricateCore(kind);
	}
	protected SyntaxToken FabricateCore(SyntaxKind kind)
	{
		Debug.Assert(Tokens.Count > 0, "Must have at least one token representing the end of the input.");
		ISyntaxNode expected = Current ?? Tokens.Last();

		IndexedPositionRange position = new(expected.FullPosition.Start, expected.FullPosition.Start);
		return new(kind, position);
	}

	protected void SkipToEndOfInput()
	{
		while (RealisticHasRemaining && Current.Kind != SyntaxKind.EndOfInput)
			SkipCurrent();

		Debug.Assert(Current?.Kind == SyntaxKind.EndOfInput);
	}
	protected void SkipCurrent()
	{
		if (Current is null || Current.Kind == SyntaxKind.EndOfInput)
			return;

		if (Next is null)
			ThrowHelper.ThrowInvalidOperationException("The very last token (which should be the special end of input token) cannot be skipped as it is required for error recovery.");

		bool hadCurrent = Current is not null;
		ISyntaxNode? badSyntax = TryParseBadSyntax();

		if (badSyntax is null && hadCurrent)
			ThrowHelper.ThrowInvalidOperationException($"{nameof(TryParseBadSyntax)}() can only return null if there's no current token.");

		if (Current is null)
			ThrowHelper.ThrowInvalidOperationException($"{nameof(TryParseBadSyntax)}() shouldn't consume the very last token (which should be the special end of input token).");

		if (badSyntax is null)
		{
			Debug.Assert(IsAtEnd);
			return;
		}

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
	protected abstract void ReportInfiniteLoop(IndexedPositionRange position);
	protected abstract void ReportExpectedToken(IndexedPositionRange position, SyntaxKind kind, string message);
	#endregion
}
