namespace OwlDomain.ParsingTools.Parsing;

/// <summary>
/// 	Represents a parser.
/// </summary>
public interface IParser : IDiagnosticProvider
{
	#region Methods
	/// <summary>Parses the result of the lexing operation.</summary>
	/// <param name="lexerResult">The result of the lexing operation.</param>
	/// <returns>The result of the parsing operation.</returns>
	IParserResult Parse(ILexerResult lexerResult);
	#endregion
}

/// <summary>
/// 	Represents a parser.
/// </summary>
/// <typeparam name="TResult">The type of the parser result.</typeparam>
public interface IParser<out TResult> : IParser
	where TResult : notnull, IParserResult
{
	#region Methods
	/// <summary>Parses the result of the lexing operation.</summary>
	/// <param name="lexerResult">The result of the lexing operation.</param>
	/// <returns>The result of the parsing operation.</returns>
	new TResult Parse(ILexerResult lexerResult);
	IParserResult IParser.Parse(ILexerResult lexerResult) => Parse(lexerResult);
	#endregion
}


/// <summary>
/// 	Represents a parser.
/// </summary>
/// <typeparam name="TResult">The type of the parser result.</typeparam>
/// <typeparam name="TTree">The type of the concrete syntax tree (CST) that is being parsed.</typeparam>
public interface IParser<out TResult, out TTree> : IParser<TResult>
	where TResult : notnull, IParserResult<TTree>
	where TTree : notnull, IConcreteSyntaxTree
{
}

/// <summary>
///	Represents the base implementation for a parser.
/// </summary>
/// <typeparam name="TResult">The type of the parser result.</typeparam>
/// <typeparam name="TTree">The type of the concrete syntax tree (CST) in the parsed document.</typeparam>
public abstract class BaseParser<TResult, TTree> : IParser<TResult, TTree>
	where TResult : notnull, IParserResult<TTree>
	where TTree : notnull, IConcreteSyntaxTree
{
	#region Nested types
	/// <summary>
	///	Represents the parser instance that can be used for a single parsing operation.
	/// </summary>
	protected abstract class ParserInstance : StageInstance
	{
		#region Nested types
		/// <summary>
		/// 	Represents a scope that's used for guarding against infinite loops that can occur from fabricating tokens.
		/// </summary>
		/// <param name="parser">The parser instance.</param>
		/// <param name="token">The token that the parser is on at the start of the loop iteration.</param>
		/// <param name="reportInfiniteLoop">
		/// 	An action that will report a diagnostic for an infinite loop, this will only be used if no diagnostics were reported.
		/// 	Should be treated as an error in your parser if this is ever called.
		/// </param>
		protected readonly struct LoopGuardScope(ParserInstance parser, ITokenNode token, Action<IndexedPositionRange> reportInfiniteLoop) : IDisposable
		{
			#region Properties
			private ParserInstance Parser { get; } = parser;
			private ITokenNode Token { get; } = token;
			private int OldDiagnosticCount { get; } = parser.Diagnostics.Count;
			private Action<IndexedPositionRange> ReportInfiniteLoop { get; } = reportInfiniteLoop;
			#endregion

			#region Methods
			/// <inheritdoc/>
			public void Dispose()
			{
				if (Parser.Current == Token)
				{
					if (OldDiagnosticCount >= Parser.Diagnostics.Count)
						ReportInfiniteLoop.Invoke(Token.Position);

					Parser.SkipCurrent();
				}
			}
			#endregion
		}
		#endregion

		#region Fields
		private int _index;
		private readonly List<ITokenNode> _tokens;
		#endregion

		#region Properties
		/// <summary>The source file that is being parsed.</summary>
		protected ISourceFile Source { get; }

		/// <summary>The tokens that should be parsed.</summary>
		/// <remarks>The parser might mutate some of the tokens for error recovery purposes.</remarks>
		protected IReadOnlyList<ITokenNode> Tokens => _tokens;

		/// <summary>The current token.</summary>
		protected ITokenNode? Current => Peek(0);

		/// <summary>The next token.</summary>
		protected ITokenNode? Next => Peek(1);

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
		/// <summary>Populates the parser instance properties.</summary>
		/// <param name="parser">The parser that created this instance.</param>
		/// <param name="lexerResult">The lexing result to parse.</param>
		protected ParserInstance(IParser parser, ILexerResult lexerResult) : base(parser)
		{
			if (lexerResult.Tokens.LastOrDefault()?.Kind != SyntaxKind.EndOfInput)
				ThrowHelper.ThrowArgumentException(nameof(lexerResult), $"Expected the lexer result to have a {nameof(SyntaxKind.EndOfInput)} token as the last token.");

			Source = lexerResult.Source;
			_tokens = [.. lexerResult.Tokens];
		}
		#endregion

		#region Methods
		/// <summary>Parses the lexed tokens.</summary>
		/// <returns>The result of the parsing operation.</returns>
		public TResult Parse()
		{
			Stopwatch watch = Stopwatch.StartNew();

			TTree tree = ParseTree();
			TResult result = CreateResult(watch.Elapsed, tree);

			return result;
		}

		/// <summary>Creates the parser result.</summary>
		/// <param name="duration">The amount of time it took to parse the concrete syntax <paramref name="tree"/> (CST).</param>
		/// <param name="tree">The concrete syntax tree (CST) that was parsed.</param>
		/// <returns>The created parser result.</returns>
		protected abstract TResult CreateResult(TimeSpan duration, TTree tree);

		/// <summary>Parses the concrete syntax tree (CST) in the source document.</summary>
		/// <returns>The parsed concrete syntax tree (CST).</returns>
		protected abstract TTree ParseTree();

		/// <summary>Gets the token at the given <paramref name="offset"/> from the current position.</summary>
		/// <param name="offset">The offset in terms of tokens.</param>
		/// <returns>The token at the given offset, or <see langword="null"/> if the end of the input was reached.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the given <paramref name="offset"/> is less than zero.</exception>
		protected ITokenNode? Peek(int offset)
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

		/// <summary>Advances the parser if the current token matches the given <paramref name="kind"/>.</summary>
		/// <param name="kind">The kind of the token to match.</param>
		/// <param name="token">The matched token.</param>
		/// <returns>
		/// 	<see langword="true"/> if the current token matched the given <paramref name="kind"/>
		/// 	and the parser was advanced, <see langword="false"/> otherwise.
		/// </returns>
		[MemberNotNullWhen(true, nameof(Current))]
		protected bool Match(SyntaxKind kind, [NotNullWhen(true)] out ITokenNode? token)
		{
			if (Current?.Kind == kind)
			{
				token = Current;
				Advance();

				return true;
			}

			token = default;
			return false;
		}

		/// <summary>Tries to match the current token against any of the given <paramref name="kinds"/>.</summary>
		/// <param name="kinds">The kinds of syntax kinds to try and match the current token against.</param>
		/// <param name="token">The matched token, or <see langword="null"/> if the current token didn't match any of the given <paramref name="kinds"/>.</param>
		/// <returns>
		/// 	<see langword="true"/> if the current token matched any of the given <paramref name="kinds"/>
		/// 	and the parser was advanced, <see langword="false"/> otherwise.
		/// </returns>
		[MemberNotNullWhen(true, nameof(Current))]
		protected bool MatchAny(ReadOnlySpan<SyntaxKind> kinds, [NotNullWhen(true)] out ITokenNode? token)
		{
			ITokenNode? current = Current;
			if (current is null)
			{
				token = default;
				return false;
			}

			Debug.Assert(Current is not null);

			foreach (SyntaxKind kind in kinds)
			{
				if (current.Kind == kind)
				{
					token = current;
					return true;
				}
			}

			token = default;
			return false;
		}

		/// <summary>Consumes the current token if it matches the given <paramref name="kind"/>, otherwise fabricated the expected token.</summary>
		/// <param name="kind">The kind of the token is expected.</param>
		/// <param name="message">The message explaining why the token was expected.</param>
		/// <returns>The matched token, or the fabricated one if matching failed.</returns>
		protected ITokenNode Expect(SyntaxKind kind, string message)
		{
			if (Match(kind, out ITokenNode? current))
				return current;

			Debug.Assert(Tokens.Count > 0, "Must have at least one token representing the end of the input.");
			ITokenNode expected = current ?? Tokens.Last();

			ReportExpectedToken(expected.Position, kind, message);
			return new FabricatedTokenNode(kind, expected.Position);
		}

		/// <summary>Marks the end of the parsed input.</summary>
		/// <returns>The token representing the end of the input.</returns>
		/// <remarks>Any unparsed tokens will be handled by this method, however reporting the diagnostics for unexpected syntax is the callers responsibility.</remarks>
		protected ITokenNode ExpectEndOfInput()
		{
			while (Current?.Kind != SyntaxKind.EndOfInput)
				SkipCurrent();

			Debug.Assert(Current.Kind == SyntaxKind.EndOfInput);
			return Expect(SyntaxKind.EndOfInput, "Expected the end of input.");
		}

		/// <summary>Skips the current token and converts it into trivia.</summary>
		/// <exception cref="InvalidOperationException">Thrown if called on the very last token.</exception>
		protected void SkipCurrent()
		{
			if (Next is null)
				ThrowHelper.ThrowInvalidOperationException("The very last token (which should be the special end of input token) cannot be skipped as it is required for error recovery.");

			ITokenNode next = Next;
			ITokenNode? current = Current;
			Debug.Assert(current is not null);

			FabricatedTriviaNode<IConcreteSyntaxNode> newTrivia = new(SyntaxKind.BadSyntax, current.Position, current);
			TriviaList newTriviaList = new(
			[
				newTrivia,
				.. next.LeadingTrivia
			]);

			_tokens[_index + 1] = next.ReplaceLeadingTrivia(newTriviaList);
			_index++;
		}

		/// <summary>Enters a scope for guarding against infinite loops that can occur during parsing from fabricating tokens.</summary>
		/// <returns>A scope to use for the loop guard.</returns>
		protected LoopGuardScope LoopGuard()
		{
			if (Current is null)
				ThrowHelper.ThrowInvalidDataException("Expected the current token to be available.");

			return new(this, Current, ReportInfiniteLoop);
		}

		/// <summary>Wraps a <paramref name="body"/> where each iteration is inside a loop guard scope.</summary>
		/// <param name="condition">The condition for each iteration.</param>
		/// <param name="body">The body of the iteration.</param>
		protected void LoopGuard(Func<bool> condition, Action body)
		{
			while (condition.Invoke())
			{
				using (LoopGuard())
					body.Invoke();
			}
		}
		#endregion

		#region Diagnostic methods
		/// <summary>Reports that an unaccounted for infinite loop has occurred as a result of fabricating tokens during parsing.</summary>
		/// <param name="position">The position where the infinite loop occurred.</param>
		/// <remarks>
		/// 	This happening means you have an error somewhere in your parser where you do node checks, but don't report any errors in some situation.
		/// </remarks>
		protected abstract void ReportInfiniteLoop(IndexedPositionRange position);

		/// <summary>Reports a diagnostic about a token that was expected at the given <paramref name="position"/>.</summary>
		/// <param name="position">The position where the token was expected.</param>
		/// <param name="kind">The kind of the token that was expected.</param>
		/// <param name="message">The message explaining why the token was expected.</param>
		protected virtual void ReportExpectedToken(IndexedPositionRange position, SyntaxKind kind, string message)
		{
			Diagnostics.Add(new Diagnostic()
			{
				Provider = DiagnosticProvider,
				Kind = DiagnosticKind.Error,
				Id = "expected_token",

				Location = new DiagnosticSourceLocation(Source, position),
				Message = message
			});
		}
		#endregion
	}
	#endregion

	#region Properties
	/// <inheritdoc/>
	public string Name => "parser";
	#endregion

	#region Methods
	/// <inheritdoc/>
	public TResult Parse(ILexerResult lexerResult)
	{
		ParserInstance parser = CreateParser(lexerResult);

		return parser.Parse();
	}

	/// <summary>Creates a new parser instance.</summary>
	/// <param name="lexerResult">The lexing result to parse.</param>
	/// <returns>The parser instance to use for the parsing operation.</returns>
	protected abstract ParserInstance CreateParser(ILexerResult lexerResult);
	#endregion
}
