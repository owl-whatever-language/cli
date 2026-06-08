namespace OwlDomain.Owl.CLI.CodeAnalysis.Parsing;

public sealed class Parser : BaseParser<ConcreteDocumentSyntax>
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
		public override IParserResult<ConcreteDocumentSyntax> Parse()
		{
			IReadOnlyList<IConcreteStatement> statements = ParseDocumentStatements();

			Debug.Assert(Current is not null);
			if (Current.Kind != SyntaxKind.EndOfInput)
				ReportExpectedStatement(Current.Position);

			ITokenNode eoi = ExpectEndOfInput();
			ConcreteDocumentSyntax document = new(statements, eoi);

			return new ParserResult<ConcreteDocumentSyntax>(Source, Diagnostics, document);
		}
		#endregion

		#region Statement methods
		private IReadOnlyList<IConcreteStatement> ParseDocumentStatements()
		{
			List<IConcreteStatement> statements = [];

			while (HasRemaining && Current.Kind != SyntaxKind.EndOfInput)
			{
				IConcreteStatement statement = ParseStatement();
				// Todo(Nightowl): Add check to ensure we parsed at least some tokens so we don't get stuck;

				statements.Add(statement);
			}

			return statements;
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
			ITokenNode terminator = Expect(SyntaxKind.Semicolon, "Expected a semicolon ';' to end an expression statement.");

			return new ConcreteExpressionStatement(expression, terminator);
		}
		private IConcreteStatement ParseVariableDeclaration()
		{
			ITokenNode type = Expect(SyntaxKind.Identifier, "Expected the type name.");
			ITokenNode variable = Expect(SyntaxKind.Identifier, "Expected the variable name.");
			ITokenNode assignment = Expect(SyntaxKind.EqualSign, "Expected the equal sign '=' before the variable's value.");
			IConcreteExpression value = ParseExpression();
			ITokenNode terminator = Expect(SyntaxKind.Semicolon, "Expected a semicolon ';' to end a variable declaration.");

			return new ConcreteVariableDeclarationStatement(type, variable, assignment, value, terminator);
		}
		#endregion

		#region Expression methods
		private IConcreteExpression ParseExpression(ExpressionPower precedence = default)
		{
			IConcreteExpression value = ParseLiteral();

			while (HasRemaining)
			{
				ExpressionPower power = ExpressionPower.PowerOf(Current.Kind);
				if (precedence.Value >= power.Value)
					break;

				if (Current.Kind == SyntaxKind.OpenBracket)
					value = ParseCallExpression(value, power);
			}

			return value;
		}
		private IConcreteExpression ParseLiteral()
		{
			if (Match(SyntaxKind.StringLiteral, out ITokenNode? stringLiteral))
				return new ConcreteLiteralExpression(stringLiteral);

			ITokenNode identifier = Expect(SyntaxKind.Identifier, "Expected an expression.");
			if (identifier is ITokenNode<string> typedIdentifier)
				return new ConcreteAccessExpression(typedIdentifier);

			return new ConcreteLiteralExpression(identifier);
		}
		private IConcreteExpression ParseCallExpression(IConcreteExpression expression, ExpressionPower power)
		{
			ITokenNode open = Expect(SyntaxKind.OpenBracket, "Expected a function call to use an opening bracket '(' before the arguments.");

			IConcreteExpression value = ParseExpression(power);
			ITokenNode close = Expect(SyntaxKind.CloseBracket, "Expected a closing bracket ')' to end a function call.");

			return new ConcreteInvocationExpression(expression, open, value, close);
		}
		#endregion

		#region Diagnostic methods
		private void ReportExpectedStatement(IndexedPositionRange position)
		{
			Diagnostics.Add(new Diagnostic()
			{
				Id = "expected_statement",
				Kind = DiagnosticKind.Error,
				Provider = Parser,
				Location = new DiagnosticSourceLocation(Source, position),
				Message = "Expected a statement."
			});
		}
		#endregion
	}
	#endregion

	#region Methods
	protected override ParserInstance CreateParser(ILexerResult lexerResult) => new Instance(this, lexerResult);
	#endregion
}
