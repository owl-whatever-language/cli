namespace OwlDomain.Owl.CLI.CodeAnalysis.Lexing;

public sealed class Lexer : BaseLexer
{
	#region Nested types
	private sealed class Instance : LexerInstance
	{
		#region Constructors
		public Instance(ILexer lexer, ISourceFile source, ITextParser text) : base(lexer, source, text)
		{
		}
		#endregion

		#region Methods
		protected override bool LexTokens()
		{
			return
				TryLexSimpleToken("(", SyntaxKind.OpenBracket) ||
				TryLexSimpleToken(")", SyntaxKind.CloseBracket) ||
				TryLexSimpleToken("=", SyntaxKind.EqualSign) ||
				TryLexSimpleToken(";", SyntaxKind.Semicolon) ||
				TryLexSimpleToken(",", SyntaxKind.Comma) ||
				TryLexTextLiteral() ||
				TryLexIdentifier();
		}
		private bool TryLexTextLiteral()
		{
			IndexedLinePosition start = Text.Position;

			if (Text.Match('"') is false)
				return false;

			ThrowIfLexemeBuilderNotCleared();
			ThrowIfValueBuilderNotCleared();

			LexemeBuilder.Append('"');

			bool reportUnclosed = true;
			while (Text.HasRemaining)
			{
				if (Text.Match('"'))
				{
					reportUnclosed = false;
					LexemeBuilder.Append('"');

					break;
				}

				if (Text.Current.IsLineBreak)
				{
					reportUnclosed = false;
					ReportLineBreakInTextLiteral();
					break;
				}

				LexemeBuilder.Append(Text.Current.Value);
				ValueBuilder.Append(Text.Current.Value);

				Text.Advance();
			}

			if (reportUnclosed)
				ReportUnclosedTextLiteral(new(start, Text.Position));

			string lexeme = GetLexeme();
			string value = GetValue();

			FinishFullToken(out TriviaList leading, out TriviaList trailing);
			TokenNode<string> token = new(SyntaxKind.StringLiteral, new(start, Text.Position), lexeme, value, leading, trailing);
			Tokens.Add(token);

			return true;
		}
		private bool TryLexIdentifier()
		{
			ThrowIfLexemeBuilderNotCleared();
			ThrowIfValueBuilderNotCleared();

			if ((char.IsAsciiLetter(Text.Current.AsChar) || Text.Current == '_') is false)
				return false;

			IndexedLinePosition start = Text.Position;

			LexemeBuilder.Append(Text.Current.Value);
			ValueBuilder.Append(Text.Current.Value);
			Text.Advance();

			while (Text.HasRemaining && (Text.Current == '_' || char.IsAsciiLetterOrDigit(Text.Current.AsChar)))
			{
				LexemeBuilder.Append(Text.Current.Value);
				ValueBuilder.Append(Text.Current.Value);
				Text.Advance();
			}

			string lexeme = GetLexeme();
			string value = GetValue();

			FinishFullToken(out TriviaList leading, out TriviaList trailing);
			TokenNode<string> token = new(SyntaxKind.Identifier, new(start, Text.Position), lexeme, value, leading, trailing);
			Tokens.Add(token);

			return true;
		}
		#endregion

		#region Trivia methods
		protected override ITriviaNode? LexTrivia()
		{
			return
				base.LexTrivia() ??
				TryLexComment();
		}
		private ITriviaNode? TryLexComment()
		{
			IndexedLinePosition start = Text.Position;
			if (Text.MatchSequence('/', '/') is false)
				return null;

			ThrowIfLexemeBuilderNotCleared();
			ThrowIfValueBuilderNotCleared();

			LexemeBuilder.Append("//");

			while (Text.HasRemaining && (Text.Current.IsLineBreak is false))
			{
				LexemeBuilder.Append(Text.Current.Value);
				ValueBuilder.Append(Text.Current.Value);
				Text.Advance();
			}

			string lexeme = GetLexeme();
			string value = GetValue().Trim();

			return new TriviaNode<string>(SyntaxKind.Comment, new(start, Text.Position), lexeme, value);
		}
		#endregion

		#region Diagnostic methods
		private void ReportUnclosedTextLiteral(IndexedPositionRange position)
		{
			Diagnostics.Add(new Diagnostic()
			{
				Provider = DiagnosticProvider,
				Kind = DiagnosticKind.Error,
				Id = "unclosed_text_literal",

				Location = new DiagnosticSourceLocation(Source, position),
				Message = "Literal text values should be closed."
			});
		}
		private void ReportLineBreakInTextLiteral()
		{
			Diagnostics.Add(new Diagnostic()
			{
				Provider = DiagnosticProvider,
				Kind = DiagnosticKind.Error,
				Id = "line_break_in_text_literal",

				Location = new DiagnosticSourceLocation(Source, new IndexedPositionRange(Text.Position, Text.Position)),
				Message = "Literal text values do not support spanning multiple lines."
			});
		}
		protected override void ReportBadCharacterGroup(ITriviaNode badCharacterGroup)
		{
			Diagnostics.Add(new Diagnostic()
			{
				Provider = DiagnosticProvider,
				Kind = DiagnosticKind.Error,
				Id = "bad_characters",

				Location = new DiagnosticSourceLocation(Source, badCharacterGroup.Position),
				Message = "Unrecognised characters were used in the source code."
			});
		}
		#endregion
	}
	#endregion

	#region Methods
	protected override LexerInstance CreateLexer(ISourceFile source, ITextParser text) => new Instance(this, source, text);
	#endregion
}
