namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing;

public sealed class ParserResult : BaseParserResult<ConcreteSyntaxTree>
{
	#region Properties
	public ParserResult(IDiagnosticBag diagnostics, TimeSpan duration, ConcreteSyntaxTree tree)
		: base(diagnostics, duration, tree)
	{
	}
	#endregion
}

public sealed class Parser : BaseParser<ParserResult, ConcreteSyntaxTree>
{
	#region Nested types
	private sealed class Instance : ParserInstance
	{
		#region Constructors
		public Instance(IParser parser, ILexerResult lexerResult) : base(parser, lexerResult)
		{
		}
		#endregion

		#region Methods
		protected override ConcreteSyntaxTree ParseTree()
		{
			IConcreteSyntaxList<IConcreteStatement> statements = ParseDocumentStatements();

			Debug.Assert(Current is not null);
			if (Current.Kind != SyntaxKind.EndOfInput)
				ReportExpectedStatement(Current.Position);

			IConcreteSyntaxToken eoi = ExpectEndOfInput();
			ConcreteDocumentSyntax document = new(statements, eoi);

			return new(Source, document);
		}
		protected override ParserResult CreateResult(TimeSpan duration, ConcreteSyntaxTree tree) => new(Diagnostics, duration, tree);
		#endregion

		#region Statement methods
		private IConcreteSyntaxList<IConcreteStatement> ParseDocumentStatements()
		{
			List<IConcreteStatement> statements = [];

			LoopGuard(() => RealisticHasRemaining, () =>
			{
				IConcreteStatement statement = ParseStatement();

				if (statement.IsFabricated is false)
					statements.Add(statement);
			});

			return new ConcreteSyntaxList<IConcreteStatement>(statements);
		}

		private IConcreteStatement ParseStatement()
		{
			if (Current?.Kind == SyntaxKind.Identifier && Next?.Kind == SyntaxKind.Identifier)
				return ParseVariableDeclaration();

			return ParseExpressionStatement();
		}
		private IConcreteStatement ParseExpressionStatement()
		{
			IConcreteExpression expression = ParseExpression();
			IConcreteSyntaxToken terminator = Expect(SyntaxKind.Semicolon, "Expected a semicolon ';' to end an expression statement.");

			return new ConcreteExpressionStatement(expression, terminator);
		}
		private IConcreteStatement ParseVariableDeclaration()
		{
			IConcreteSyntaxToken type = Expect(SyntaxKind.Identifier, "Expected the type name.");
			IConcreteSyntaxToken variable = Expect(SyntaxKind.Identifier, "Expected the variable name.");
			IConcreteSyntaxToken assignment = Expect(SyntaxKind.EqualSign, "Expected the equal sign '=' before the variable's value.");
			IConcreteExpression value = ParseExpression();
			IConcreteSyntaxToken terminator = Expect(SyntaxKind.Semicolon, "Expected a semicolon ';' to end a variable declaration.");

			return new ConcreteVariableDeclarationStatement(type, variable, assignment, value, terminator);
		}
		#endregion

		#region Expression methods
		private IConcreteExpression ParseExpression(ExpressionPower precedence = default)
		{
			IConcreteExpression expression = ParseLiteral();

			while (RealisticHasRemaining)
			{
				ExpressionPower power = ExpressionPower.PowerOf(Current.Kind);
				if (precedence.Value >= power.Value)
					break;

				if (Current.Kind == SyntaxKind.OpenBracket)
					expression = ParseInvocationExpression(expression); // Invocations don't care about the pratt parsing power for values.
			}

			return expression;
		}

		private IConcreteExpression ParseLiteral()
		{
			if (Match(SyntaxKind.StringLiteral, out IConcreteSyntaxToken? stringLiteral))
				return new ConcreteLiteralExpression(stringLiteral);

			IConcreteSyntaxToken identifier = Expect(SyntaxKind.Identifier, "Expected an expression.");
			return new ConcreteAccessExpression(identifier);
		}
		private IConcreteExpression ParseInvocationExpression(IConcreteExpression expression)
		{
			IConcreteSyntaxToken open = Expect(SyntaxKind.OpenBracket, "Expected a function call to use an opening bracket '(' before the arguments.");
			IConcreteSeparatedSyntaxList<IConcreteExpression, IConcreteSyntaxToken> values = ParseInvocationValues();
			IConcreteSyntaxToken close = Expect(SyntaxKind.CloseBracket, "Expected a closing bracket ')' to end a function call.");

			return new ConcreteInvocationExpression(expression, open, values, close);
		}
		private IConcreteSeparatedSyntaxList<IConcreteExpression, IConcreteSyntaxToken> ParseInvocationValues()
		{
			List<IConcreteSyntaxNode> nodes = [];
			List<IConcreteExpression> values = [];
			List<IConcreteSyntaxToken> separators = [];

			LoopGuard(() => RealisticHasRemaining && Current.Kind != SyntaxKind.CloseBracket, () =>
			{
				IConcreteExpression value = ParseExpression();
				nodes.Add(value);
				values.Add(value);

				if (RealisticHasRemaining && Current.Kind != SyntaxKind.CloseBracket)
				{
					IConcreteSyntaxToken separator = Expect(SyntaxKind.Comma, "Expect a comma ',' to separate function call arguments.");
					nodes.Add(separator);
					separators.Add(separator);
				}
			});

			return new ConcreteSeparatedSyntaxList<IConcreteExpression, IConcreteSyntaxToken>(nodes, values, separators);
		}
		#endregion

		#region Diagnostic methods
		private void ReportExpectedStatement(IndexedPositionRange position)
		{
			Diagnostics.Add(new Diagnostic()
			{
				Provider = DiagnosticProvider,
				Kind = DiagnosticKind.Error,
				Id = "expected_statement",

				Location = new DiagnosticSourceLocation(Source, position),
				Message = "Expected a statement."
			});
		}
		protected override void ReportInfiniteLoop(IndexedPositionRange position)
		{
			Diagnostics.Add(new Diagnostic()
			{
				Provider = DiagnosticProvider,
				Kind = DiagnosticKind.Error,
				Id = "infinite_parsing_loop",

				Location = new DiagnosticSourceLocation(Source, position),
				Message = "An unaccounted for infinite loop occurred during parsing, this is likely an error with the OWL parser."
			});
		}
		#endregion
	}
	#endregion

	#region Methods
	protected override ParserInstance CreateParser(ILexerResult lexerResult) => new Instance(this, lexerResult);
	#endregion
}
