namespace OwlDomain.ParsingTools.Parsing;

public abstract class BaseLexer
{
	#region Constants
	private const string Space2 = "  ";
	private const string Space3 = "   ";
	private const string Space4 = "    ";

	// Todo(Nightowl): Analyse these once there's actual code to analyse;
	private const int InitialLeadingTriviaCapacity = 4;
	private const int InitialTrailingTriviaCapacity = 4;
	#endregion

	#region Properties
	/// <remarks>Purposefully keep these common strings here to ensure they're interned.</remarks>
	private static HashSet<string> Interned { get; } =
	[
		"\r", "\n", "\r\n", // line breaks
		"\t", "\t\t", "\t\t\t", "\t\t\t\t", "\t\t\t\t\t",
		" ",
		// Note(Nightowl): I would like to note that I was very tempted to discriminate again space indentation here, but I decided to optimise for it anyway;
		Space2,
		Space2 + Space2,
		Space2 + Space2 + Space2,
		Space2 + Space2 + Space2 + Space2,
		Space2 + Space2 + Space2 + Space2 + Space2,
		Space3,
		Space3 + Space3,
		Space3 + Space3 + Space3,
		Space3 + Space3 + Space3 + Space3,
		Space3 + Space3 + Space3 + Space3 + Space3,
		Space4,
		Space4 + Space4,
		Space4 + Space4 + Space4,
		Space4 + Space4 + Space4 + Space4,
		Space4 + Space4 + Space4 + Space4 + Space4,
	];

	protected List<ISyntaxToken> Tokens { get; } = [];
	protected List<ISyntaxTrivia> LeadingTrivia { get; private set; } = [];
	protected List<ISyntaxTrivia> TrailingTrivia { get; private set; } = [];
	protected ITextParser Text { get; }

	/// <summary>A reusable builder that can be used for accumulating lexed text.</summary>
	/// <remarks>Make sure to clear the builder <b>after</b> using it.</remarks>
	protected StringBuilder LexemeBuilder { get; } = new();

	/// <summary>A reusable builder that can be used for accumulating lexed text for the purpose of converting it to a token/trivia value.</summary>
	/// <remarks>Make sure to clear the builder <b>after</b> using it.</remarks>
	protected StringBuilder ValueBuilder { get; } = new();
	#endregion

	#region Constructors
	protected BaseLexer(ITextParser text)
	{
		Text = text;
	}
	#endregion

	#region Methods
	protected void Lex()
	{
		while (Text.HasRemaining)
			LexTokenSequence();

		FinishFullToken(out TriviaList leading, out TriviaList trailing);
		if (trailing.Any())
			ThrowHelper.ThrowInvalidOperationException("The end of input token shouldn't have any trailing tokens.");

		LexEndOfInput(leading);
	}

	protected void LexTokenSequence()
	{
		ThrowIfLexemeBuilderNotCleared();
		ThrowIfValueBuilderNotCleared();

		IndexedLinePosition start = Text.Position;
		LexLeadingTrivia();

		if (Text.IsAtEnd)
			return;

		if (LexTokens())
		{
			if (Text.Position == start)
				ThrowHelper.ThrowInvalidOperationException("Expected the text parser to be advanced to a later position.");

			return;
		}

		LexBadCharacter();

		if (Text.Position == start)
			ThrowHelper.ThrowInvalidOperationException("Expected the text parser to be advanced to a later position.");
	}

	protected abstract bool LexTokens();
	#endregion

	#region Token methods
	/// <summary>Creates the final end of input token.</summary>
	/// <param name="leading">The list of the leading trivia nodes.</param>
	protected void LexEndOfInput(TriviaList leading)
	{
		SyntaxToken token = new(SyntaxKind.EndOfInput, new(Text.Position, Text.Position), lexeme: null, value: null, leading, TriviaList.Empty);
		Tokens.Add(token);
	}

	/// <summary>Lexes the bad character at the current position.</summary>
	/// <returns>The lexed bad character.</returns>
	protected void LexBadCharacter()
	{
		IndexedLinePosition start = Text.Position;
		string lexeme = Text.Current.Value;
		Text.Advance();

		if (LeadingTrivia.Count is 0 || LeadingTrivia[^1].Kind != SyntaxKind.BadCharactersTrivia)
		{
			IndexedPositionRange position = new(start, Text.Position);
			SyntaxTrivia trivia = new(SyntaxKind.BadCharactersTrivia, position, lexeme);

			LeadingTrivia.Add(trivia);
		}
		else
		{
			ISyntaxTrivia last = LeadingTrivia[^1];
			SyntaxTrivia trivia = new(SyntaxKind.BadCharactersTrivia, new(last.Position.Start, Text.Position), last.Lexeme + lexeme);
			LeadingTrivia[^1] = trivia;
		}
	}

	/// <summary>Tries to lex a simple token that consists of the given <paramref name="sequence"/>.</summary>
	/// <param name="sequence">The sequence to try and lex.</param>
	/// <param name="kind">The kind of the token that is being lexed.</param>
	/// <returns><see langword="true"/> if a token was lexed and added to <see cref="Tokens"/>, <see langword="false"/> otherwise.</returns>
	/// <remarks>This method assumes that the <paramref name="sequence"/> only consists of <see cref="char"/> characters.</remarks>
	protected bool TryLexSimpleToken(string sequence, SyntaxKind kind)
	{
		IndexedLinePosition start = Text.Position;

		for (int i = 0; i < sequence.Length; i++)
		{
			if (Text.Peek(i) != sequence[i])
				return false;
		}

		Text.Advance(sequence.Length);
		IndexedLinePosition end = Text.Position;

		FinishFullToken(out TriviaList leading, out TriviaList trailing);
		SyntaxToken token = new(kind, new(start, end), sequence, value: null, leading, trailing);
		Tokens.Add(token);

		return true;
	}

	/// <summary>Performs the necessary steps to finish a full token.</summary>
	/// <param name="leadingTrivia">The final list of the leading trivia nodes.</param>
	/// <param name="trailingTrivia">The final list of the trailing trivia nodes.</param>
	protected void FinishFullToken(out TriviaList leadingTrivia, out TriviaList trailingTrivia)
	{
		LexTrailingTrivia();
		ReportCurrentBadCharacters();

		leadingTrivia = new(LeadingTrivia);
		trailingTrivia = new(TrailingTrivia);

		LeadingTrivia = new(InitialLeadingTriviaCapacity);
		TrailingTrivia = new(InitialTrailingTriviaCapacity);
	}

	/// <summary>Performs the necessary steps to finish an infix token.</summary>
	/// <param name="leadingTrivia">The final list of the leading trivia nodes.</param>
	/// <exception cref="InvalidOperationException">Thrown if any trailing trivia nodes have already been accumulated.</exception>
	protected void FinishPrefixToken(out TriviaList leadingTrivia)
	{
		if (TrailingTrivia.Any())
			ThrowHelper.ThrowInvalidOperationException("Some trailing trivia has already been accumulated.");

		ReportCurrentBadCharacters();

		leadingTrivia = new(LeadingTrivia);
		LeadingTrivia = new(InitialLeadingTriviaCapacity);
	}

	protected void FinishSuffixToken(out TriviaList trailingTrivia)
	{
		if (LeadingTrivia.Any())
			ThrowHelper.ThrowInvalidOperationException("Some leading trivia has already been accumulated.");

		LexTrailingTrivia();
		ReportCurrentBadCharacters();

		trailingTrivia = new(TrailingTrivia);
		TrailingTrivia = new(InitialTrailingTriviaCapacity);
	}

	protected void FinishInfixToken()
	{
		if (LeadingTrivia.Any())
			ThrowHelper.ThrowInvalidOperationException("Some leading trivia has already been accumulated.");

		if (TrailingTrivia.Any())
			ThrowHelper.ThrowInvalidOperationException("Some trailing trivia has already been accumulated.");
	}

	private void ReportCurrentBadCharacters()
	{
		foreach (ISyntaxTrivia trivia in LeadingTrivia)
		{
			if (trivia.Kind == SyntaxKind.BadCharactersTrivia)
				ReportBadCharacters(trivia);
		}
	}
	protected abstract void ReportBadCharacters(ISyntaxTrivia badGroup);
	#endregion

	#region Trivia methods
	protected virtual void LexLeadingTrivia()
	{
		if (LeadingTrivia.Any(static t => t.Kind != SyntaxKind.BadCharactersTrivia))
			ThrowHelper.ThrowInvalidOperationException("Tried to lex leading trivia nodes when the previously lexed ones still weren't used.");

		ISyntaxTrivia? node = LexTrivia();
		while (node is not null)
		{
			LeadingTrivia.Add(node);
			node = LexTrivia();
		}
	}
	protected virtual void LexTrailingTrivia()
	{
		if (TrailingTrivia.Any())
			ThrowHelper.ThrowInvalidOperationException("Tried to lex trailing trivia nodes when the previously lexed ones still weren't used.");

		ISyntaxTrivia? node = LexTrivia();
		while (node is not null)
		{
			TrailingTrivia.Add(node);
			if (node.Kind == SyntaxKind.LineBreak)
				break;

			node = LexTrivia();
		}
	}
	protected virtual ISyntaxTrivia? LexTrivia()
	{
		IndexedLinePosition start = Text.Position;

		if (Text.MatchAny(["\r", "\n", "\r\n"], out string? match))
		{
			IndexedLinePosition end = new(start.Index + 1, start.Line, start.Column + 1);
			IndexedPositionRange position = new(start, end);

			return new SyntaxTrivia(SyntaxKind.LineBreak, position, match, ClassificationKind.LineBreak);
		}

		if (Text.IsAtStartOfLine)
		{
			if (Text.Current == ' ')
				return LexIndentation(' ');

			if (Text.Current == '\t')
				return LexIndentation('\t');
		}

		if (Text.Current.IsWhiteSpace)
			return LexWhiteSpace();

		return null;
	}
	protected virtual ISyntaxTrivia LexIndentation(char character)
	{
		ThrowIfLexemeBuilderNotCleared();

		IndexedLinePosition start = Text.Position;

		while (Text.Current.IsWhiteSpace)
		{
			LexemeBuilder.Append(Text.Current.Value);
			Text.Advance();
		}

		string lexeme = GetLexeme().TryIntern();

		return new SyntaxTrivia(SyntaxKind.WhiteSpace, new(start, Text.Position), lexeme, ClassificationKind.Indentation);
	}
	private ISyntaxTrivia LexWhiteSpace()
	{
		ThrowIfLexemeBuilderNotCleared();

		IndexedLinePosition start = Text.Position;

		while (Text.Current.IsWhiteSpace)
		{
			LexemeBuilder.Append(Text.Current.Value);
			Text.Advance();
		}

		string lexeme = GetLexeme().TryIntern();

		return new SyntaxTrivia(SyntaxKind.WhiteSpace, new(start, Text.Position), lexeme, ClassificationKind.Whitespace);
	}
	#endregion

	#region Helpers
	/// <summary>Checks if the <see cref="LexemeBuilder"/> was cleared.</summary>
	/// <exception cref="InvalidOperationException">Thrown if the <see cref="LexemeBuilder"/> was not cleared.</exception>
	protected void ThrowIfLexemeBuilderNotCleared()
	{
		if (LexemeBuilder.Length > 0)
			ThrowHelper.ThrowInvalidOperationException("The lexeme builder was not cleared after being used.");
	}

	/// <summary>Checks if the <see cref="ValueBuilder"/> was cleared.</summary>
	/// <exception cref="InvalidOperationException">Thrown if the <see cref="ValueBuilder"/> was not cleared.</exception>
	protected void ThrowIfValueBuilderNotCleared()
	{
		if (ValueBuilder.Length > 0)
			ThrowHelper.ThrowInvalidOperationException("The value builder was not cleared after being used.");
	}

	/// <summary>Gets the value from the <see cref="LexemeBuilder"/> and then clears it.</summary>
	/// <returns>The text that was accumulated in the <see cref="LexemeBuilder"/>.</returns>
	protected string GetLexeme()
	{
		string lexeme = LexemeBuilder.ToString();
		LexemeBuilder.Clear();

		return lexeme;
	}

	/// <summary>Gets the value from the <see cref="ValueBuilder"/> and then clears it.</summary>
	/// <returns>The text that was accumulated in the <see cref="ValueBuilder"/>.</returns>
	protected string GetValue()
	{
		string lexeme = ValueBuilder.ToString();
		ValueBuilder.Clear();

		return lexeme;
	}
	#endregion
}
