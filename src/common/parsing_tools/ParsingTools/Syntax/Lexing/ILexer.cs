namespace OwlDomain.ParsingTools.Syntax.Lexing;

/// <summary>
/// 	Represents a lexer.
/// </summary>
public interface ILexer : IDiagnosticProvider
{
	#region Methods
	/// <summary>Lexes the given <paramref name="source"/> file.</summary>
	/// <param name="source">The source file to lex.</param>
	/// <returns>The result of the lexing operation.</returns>
	ILexerResult Lex(ISourceFile source);
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a lexer.
/// </summary>
public abstract class BaseLexer : ILexer
{
	#region Nested types
	/// <summary>
	///	Represents the lexer instance that can be used for a single lexing operation.
	/// </summary>
	protected abstract class LexerInstance
	{
		#region Properties
		/// <summary>The lexer that created this instance.</summary>
		protected ILexer Lexer { get; }

		/// <summary>The source file that is being lexed.</summary>
		protected ISourceFile Source { get; }

		/// <summary>The text parser for the source file that is being lexed.</summary>
		protected ITextParser Text { get; }

		/// <summary>The diagnostics that have occurred during the lexing operation.</summary>
		protected DiagnosticBag Diagnostics { get; } = [];

		/// <summary>The lexed tokens.</summary>
		protected List<ITokenNode> Tokens { get; } = [];

		/// <summary>The currently accumulated leading trivia nodes.</summary>
		protected List<ITriviaNode> LeadingTrivia { get; } = [];

		/// <summary>The currently accumulated trailing trivia nodes.</summary>
		protected List<ITriviaNode> TrailingTrivia { get; } = [];

		/// <summary>A reusable builder that can be used for accumulating lexed text.</summary>
		/// <remarks>Make sure to clear the builder <b>after</b> using it.</remarks>
		protected StringBuilder LexemeBuilder { get; } = new();

		/// <summary>A reusable builder that can be used for accumulating lexed text for the purpose of converting it to a token/trivia value.</summary>
		/// <remarks>Make sure to clear the builder <b>after</b> using it.</remarks>
		protected StringBuilder ValueBuilder { get; } = new();
		private bool EncounteredBadCharacter { get; set; } = false;
		#endregion

		#region Constructors
		/// <summary>Populates the lexer instance properties.</summary>
		/// <param name="lexer">The lexer that created this instance.</param>
		/// <param name="source">The source file that is being lexed.</param>
		/// <param name="text">The text parser for the <paramref name="source"/> file that is being lexed.</param>
		protected LexerInstance(ILexer lexer, ISourceFile source, ITextParser text)
		{
			Lexer = lexer;
			Source = source;
			Text = text;
		}
		#endregion

		#region Methods
		/// <summary>Lexes the source file.</summary>
		/// <returns>The result of the lexing operation.</returns>
		public ILexerResult Lex()
		{
			Stopwatch watch = Stopwatch.StartNew();

			while (Text.HasRemaining)
			{
				ThrowIfLexemeBuilderNotCleared();
				ThrowIfValueBuilderNotCleared();

				IndexedLinePosition start = Text.Position;

				LexLeadingTrivia();

				if (Text.IsAtEnd)
					break;

				if (LexTokens())
				{
					if (Text.Position == start)
						ThrowHelper.ThrowInvalidOperationException("Expected the text parser to be advanced to a later position.");

					continue;
				}

				LexBadCharacter();

				if (Text.Position == start)
					ThrowHelper.ThrowInvalidOperationException("Expected the text parser to be advanced to a later position.");
			}

			FinishFullToken(out TriviaList leading, out TriviaList trailing);
			if (trailing.Any())
				ThrowHelper.ThrowInvalidOperationException("The end of input token shouldn't have any trailing tokens.");

			LexEndOfInput(leading);
			IReadOnlyList<ITokenNode> finalTokens = FixTokens();

			return new LexerResult(Source, finalTokens, Diagnostics, watch.Elapsed);
		}
		#endregion

		#region Token methods
		/// <summary>Creates the final end of input token.</summary>
		/// <param name="leading">The list of the leading trivia nodes.</param>
		protected virtual void LexEndOfInput(TriviaList leading)
		{
			FabricatedTokenNode token = new(SyntaxKind.EndOfInput, new(Text.Position, Text.Position), leading);
			Tokens.Add(token);
		}

		/// <summary>Lexes the bad character at the current position.</summary>
		/// <returns>The lexed bad character.</returns>
		protected virtual void LexBadCharacter()
		{
			EncounteredBadCharacter = true;
			IndexedLinePosition start = Text.Position;
			string lexeme = Text.Current.Value;
			Text.Advance();

			IndexedPositionRange position = new(start, Text.Position);
			FinishFullToken(out TriviaList leading, out TriviaList trailing);

			TokenNode bad = new(SyntaxKind.BadCharacter, position, lexeme, leading, trailing);
			Tokens.Add(bad);
		}

		/// <summary>Performs the necessary steps to finish a full token.</summary>
		/// <param name="leadingTrivia">The final list of the leading trivia nodes.</param>
		/// <param name="trailingTrivia">The final list of the trailing trivia nodes.</param>
		protected virtual void FinishFullToken(out TriviaList leadingTrivia, out TriviaList trailingTrivia)
		{
			LexTrailingTrivia();

			if (LeadingTrivia.Count is 0)
				leadingTrivia = [];
			else
			{
				leadingTrivia = new(LeadingTrivia);
				LeadingTrivia.Clear();
			}

			if (TrailingTrivia.Count is 0)
				trailingTrivia = [];
			else
			{
				trailingTrivia = new(TrailingTrivia);
				TrailingTrivia.Clear();
			}
		}

		/// <summary>Performs the necessary steps to finish an infix token.</summary>
		/// <param name="leadingTrivia">The final list of the leading trivia nodes.</param>
		/// <exception cref="InvalidOperationException">Thrown if any trailing trivia nodes have already been accumulated.</exception>
		protected virtual void FinishInfixToken(out TriviaList leadingTrivia)
		{
			if (TrailingTrivia.Any())
				ThrowHelper.ThrowInvalidOperationException("Some trailing trivia has already been accumulated.");

			if (LeadingTrivia.Count is 0)
			{
				leadingTrivia = [];
				return;
			}

			leadingTrivia = new(LeadingTrivia);
			LeadingTrivia.Clear();
		}

		/// <summary>Lexes the next tokens.</summary>
		/// <returns><see langword="true"/> if anything was lexed, <see langword="false"/> otherwise.</returns>
		protected abstract bool LexTokens();

		/// <summary>Fixes the bad character tokens by grouping them up and turning them into trivia nodes.</summary>
		/// <returns>A list of the fixed up tokens to use as the final lexing result.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the <see cref="LexemeBuilder"/> or the <see cref="ValueBuilder"/> was not cleared before calling the method.</exception>
		protected virtual IReadOnlyList<ITokenNode> FixTokens()
		{
			ThrowIfLexemeBuilderNotCleared();
			ThrowIfValueBuilderNotCleared();

			if (EncounteredBadCharacter is false)
				return Tokens;

			List<ITokenNode> tokens = [];
			Queue<ITokenNode> badTokens = [];

			foreach (ITokenNode token in Tokens)
			{
				if (token.Kind == SyntaxKind.BadCharacter)
				{
					badTokens.Enqueue(token);
					continue;
				}

				if (badTokens.Count is 0)
				{
					tokens.Add(token);
					continue;
				}

				List<ITriviaNode> leading = [.. token.LeadingTrivia];
				int index = 0;

				while (badTokens.Any())
				{
					IReadOnlyList<ITokenNode> touching = GroupTouching(badTokens);
					Debug.Assert(touching.Any());

					TriviaList badLeading = touching[0].LeadingTrivia;
					TriviaList badTrailing = touching[^1].TrailingTrivia;

					leading.InsertRange(index, badLeading);
					index += badLeading.Count;

					foreach (ITokenNode current in touching)
						LexemeBuilder.Append(current.Lexeme);

					string badLexeme = GetLexeme();

					ITriviaNode badGroup = new FabricatedTriviaNode(
						SyntaxKind.BadCharactersTrivia,
						new(touching[0].Position.Start, touching[^1].Position.End),
						badLexeme);

					ReportBadCharacterGroup(badGroup);

					leading.Insert(index++, badGroup);

					leading.InsertRange(index, badTrailing);
					index += badTrailing.Count;
				}

				ITokenNode newToken = token.ReplaceLeadingTrivia(new(leading));
				tokens.Add(newToken);
			}

			return tokens;
		}

		/// <summary>Groups together the next tokens that are touching.</summary>
		/// <param name="badTokens">The queue of the bad tokens.</param>
		/// <returns>A list of the grouped bad tokens.</returns>
		/// <exception cref="ArgumentException">thrown if the <paramref name="badTokens"/> queue was empty.</exception>
		protected virtual IReadOnlyList<ITokenNode> GroupTouching(Queue<ITokenNode> badTokens)
		{
			Guard.IsNotEmpty(badTokens);

			ITokenNode last = badTokens.Dequeue();
			List<ITokenNode> touching = [last];

			while (badTokens.Any())
			{
				ITokenNode current = badTokens.Peek();

				if (last.TrailingTrivia.Any() || current.LeadingTrivia.Any())
					return touching;

				last = badTokens.Dequeue();
				touching.Add(last);
			}

			return touching;
		}

		/// <summary>Creates a diagnostic for the bad character group.</summary>
		/// <param name="badCharacterGroup">The trivia holding the bad characters.</param>
		protected abstract void ReportBadCharacterGroup(ITriviaNode badCharacterGroup);

		/// <summary>Tries to lex a simple token that consists of the given <paramref name="sequence"/>.</summary>
		/// <param name="sequence">The sequence to try and lex.</param>
		/// <param name="kind">The kind of the token that is being lexed.</param>
		/// <returns><see langword="true"/> if a token was lexed and added to <see cref="Tokens"/>, <see langword="false"/> otherwise.</returns>
		protected bool TryLexSimpleToken(string sequence, SyntaxKind kind)
		{
			IndexedLinePosition start = Text.Position;

			if (Text.MatchSequence(sequence) is false)
				return false;

			FinishFullToken(out TriviaList leading, out TriviaList trailing);
			TokenNode token = new(kind, new(start, Text.Position), sequence, leading, trailing);
			Tokens.Add(token);

			return true;
		}
		#endregion

		#region Trivia methods
		/// <summary>Accumulates the leading trivia nodes.</summary>
		protected void LexLeadingTrivia()
		{
			if (LeadingTrivia.Any())
				ThrowHelper.ThrowInvalidOperationException("Tried to lex leading trivia when the previously lexed ones still weren't used.");

			ITriviaNode? node = LexTrivia();
			while (node is not null)
			{
				LeadingTrivia.Add(node);
				node = LexTrivia();
			}
		}

		/// <summary>Accumulates the trailing trivia nodes.</summary>
		protected void LexTrailingTrivia()
		{
			if (TrailingTrivia.Any())
				ThrowHelper.ThrowInvalidOperationException("Tried to lex trailing trivia when the previously lexed ones still weren't used.");

			ITriviaNode? node = LexTrivia();
			while (node is not null)
			{
				TrailingTrivia.Add(node);
				if (node.Kind == SyntaxKind.LineBreak)
					break;

				node = LexTrivia();
			}
		}

		/// <summary>Lexes the next trivia node.</summary>
		/// <returns>The lexed trivia node, or <see langword="null"/> if no more trivia nodes could be lexed.</returns>
		protected virtual ITriviaNode? LexTrivia()
		{
			ThrowIfLexemeBuilderNotCleared();

			IndexedLinePosition start = Text.Position;

			if (Text.MatchAny(["\r", "\n", "\r\n"], out string? match))
				return new TriviaNode(SyntaxKind.LineBreak, new(start, Text.Position), match);

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

		private ITriviaNode LexIndentation(char character)
		{
			IndexedLinePosition start = Text.Position;

			while (Text.Match(character))
				LexemeBuilder.Append(character);

			string lexeme = GetLexeme();

			return new TriviaNode(SyntaxKind.Indentation, new(start, Text.Position), lexeme);
		}
		private ITriviaNode LexWhiteSpace()
		{
			IndexedLinePosition start = Text.Position;

			while (Text.Current.IsWhiteSpace)
			{
				LexemeBuilder.Append(Text.Current.Value);
				Text.Advance();
			}

			string lexeme = GetLexeme();

			return new TriviaNode(SyntaxKind.WhiteSpace, new(start, Text.Position), lexeme);
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
	#endregion

	#region Properties
	/// <inheritdoc/>
	public virtual string Name => "lexer";
	#endregion

	#region Methods
	/// <inheritdoc/>
	public ILexerResult Lex(ISourceFile source)
	{
		ITextParser text = source.CreateParser();
		LexerInstance lexer = CreateLexer(source, text);

		return lexer.Lex();
	}

	/// <summary>Creates a new lexer instance.</summary>
	/// <param name="source">The source file that is being lexed.</param>
	/// <param name="text">The text parser for the <paramref name="source"/> file that is being lexed.</param>
	/// <returns>The lexer instance to use for the lexing operation.</returns>
	protected abstract LexerInstance CreateLexer(ISourceFile source, ITextParser text);
	#endregion
}
