using System.Text;

namespace OwlDomain.Owl.Code.CodeAnalysis.Parsing;

public sealed class ParsingResult : ISourceStageResult, IStageResultPerformance, IStageResultDiagnostics
{
	#region Properties
	public string Stage => "parsing";
	public ISourceFile Source { get; }
	public IDiagnosticBag Diagnostics { get; }
	public IPerformanceResult Performance { get; }
	public IConcreteSyntaxTree Tree { get; }
	#endregion

	#region Constructors
	public ParsingResult(
		ISourceFile source,
		IDiagnosticBag diagnostics,
		IPerformanceResult performance,
		IConcreteSyntaxTree tree)
	{
		Source = source;
		Diagnostics = diagnostics;
		Performance = performance;
		Tree = tree;
	}
	#endregion
}

public sealed class LexingAndParsingResult : ISourceStageResult, IStageResultPerformance, ICombinedStageResult<IStageResultPerformance>
{
	#region Properties
	public string Stage => "lexing_and_parsing";
	public ISourceFile Source => Lexing.Source;
	public IPerformanceResult Performance { get; }
	public LexingResult Lexing { get; }
	public ParsingResult Parsing { get; }
	public IReadOnlyList<IStageResultPerformance> Children => [Lexing, Parsing];
	#endregion

	#region Constructors
	public LexingAndParsingResult(
		IPerformanceResult performance,
		LexingResult lexing,
		ParsingResult parsing)
	{
		Performance = performance;
		Lexing = lexing;
		Parsing = parsing;
	}
	#endregion
}

public sealed class ParallelParsingResult : IParallelStageResult<LexingAndParsingResult>, IStagePerformanceBreakdownResult
{
	#region Properties
	public string Stage => "parsing";
	public IPerformanceResult Performance { get; }
	public IReadOnlyCollection<LexingAndParsingResult> Children { get; }
	#endregion

	#region Constructors
	public ParallelParsingResult(
		IPerformanceResult performance,
		IReadOnlyList<LexingAndParsingResult> results)
	{
		Performance = performance;
		Children = results;
	}
	#endregion

	#region Methods
	public IReadOnlyDictionary<string, IPerformanceResult> GetStagePerformanceBreakdown() => Performance.CalculateStageBreakdown(Children.SelectMany(r => r.Children));
	#endregion
}

public sealed class Parser : BaseParser, IDiagnosticProvider
{
	#region Token fragments
	private static TextFragment SemicolonFragment => new(";", ClassificationKind.Punctuation);
	private static TextFragment CommaFragment => new(",", ClassificationKind.Punctuation);
	private static TextFragment OpeningBracketFragment => new("(", ClassificationKind.Punctuation);
	private static TextFragment ClosingBracketFragment => new(")", ClassificationKind.Punctuation);
	private static TextFragment OpeningBraceFragment => new("{", ClassificationKind.Punctuation);
	private static TextFragment ClosingBraceFragment => new("}", ClassificationKind.Punctuation);
	private static TextFragment OpeningAngleBracketFragment => new("<", ClassificationKind.Punctuation);
	private static TextFragment ClosingAngleBracketFragment => new(">", ClassificationKind.Punctuation);
	private static TextFragment NumberUnderscoreFragment => new("_", ClassificationKind.Number);
	private static TextFragment EqualSignFragment => new("=", ClassificationKind.Punctuation);
	#endregion

	#region Properties
	public string Name => "parser";
	private ISourceFile Source { get; }
	protected override int DiagnosticCount => Diagnostics.Count;
	private DiagnosticBag Diagnostics { get; } = [];
	#endregion

	#region Constructors
	private Parser(ISourceFile source, IReadOnlyList<ISyntaxToken> tokens) : base(tokens)
	{
		Source = source;
	}
	#endregion

	#region Functions
	public static ParsingResult Parse(ISourceFile source, IReadOnlyList<ISyntaxToken> tokens)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			Parser parser = new(source, tokens);
			ConcreteSyntaxTree tree = parser.Parse();

			return new(source, parser.Diagnostics, performance, tree);
		}
	}
	public static LexingAndParsingResult Parse(ISourceFile source)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			LexingResult lexing = Lexer.Lex(source);
			ParsingResult parsing = Parse(source, lexing.Tokens);

			return new(performance, lexing, parsing);
		}
	}

	public static ParallelParsingResult Parse(params IReadOnlyCollection<ISourceFile> files)
	{
		using (PerformanceResult.Scope(out IPerformanceResult performance))
		{
			if (files.Count is 0)
				return new(performance, []);

			if (files.Count is 1)
			{
				LexingAndParsingResult result = Parse(files.Single());
				return new(performance, [result]);
			}

			LexingAndParsingResult[] results = new LexingAndParsingResult[files.Count];
			ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };

			Parallel.ForEach(files, options, (source, _, index) =>
			{
				LexingAndParsingResult result = Parse(source);
				results[index] = result;
			});

			return new(performance, results);
		}
	}
	#endregion

	#region Methods
	private ConcreteSyntaxTree Parse()
	{
		ConcreteDocumentSyntax document = ParseDocument();
		return new(Source, document);
	}
	protected override ISyntaxNode? TryParseBadSyntax()
	{
		// Note(Nightowl): Bad syntax parsing should only be done on things that will start with a keyword;
		ISyntaxNode? attempt = TryParseLocalFunctionDeclaration();

		if (attempt is not null)
			return attempt;

		ISyntaxToken? current = Current;
		if (current is not null)
			Advance();

		return Convert(current);
	}
	private ConcreteDocumentSyntax ParseDocument()
	{
		SyntaxList<IConcreteStatementSyntax> statements = ParseDocumentStatements();

		Debug.Assert(Current is not null);
		if (Current.Kind != SyntaxKind.EndOfInput)
			ReportExpectedSimple(Current, "statement", "Expected a statement here.");

		SkipToEndOfInput();
		IConcreteToken endOfInput = Expect(SyntaxKind.EndOfInput, token => ReportExpectedSimple(token, "eof", "Expected the end of the input."));

		return new(statements, endOfInput);
	}
	private SyntaxList<IConcreteStatementSyntax> ParseDocumentStatements() => ParseStatements();
	private SyntaxList<IConcreteStatementSyntax> ParseStatements(params ReadOnlySpan<SyntaxKind> stopAt)
	{
		List<IConcreteStatementSyntax> statements = [];

		while (RealisticHasRemaining && (IsCurrentAny(stopAt) is false))
		{
			using LoopGuardScope _ = LoopGuard();

			IConcreteStatementSyntax? statement = TryParseStatement();
			if (statement is not null)
			{
				if (statement.IsFabricated is false)
					statements.Add(statement);
			}
			else if (Current.Kind == SyntaxKind.CloseBrace && (stopAt.Contains(SyntaxKind.CloseBrace) is false))
			{
				// Note(Nightowl):
				// When possible, use the previous, correctly parsed brace
				// so that the "smart" diagnostic shows a better location;
				ISyntaxToken target = Current;
				ISyntaxToken? last = statements.LastOrDefault()?.Flatten().LastOrDefault();
				if (last?.Kind == SyntaxKind.CloseBrace)
					target = last;

				ReportDuplicate(target, "closing_brace", ClosingBraceFragment);
				SkipCurrent();
			}
			else
			{
				Debug.Assert(Current is not null, "EOF should still be here.");
				ReportExpectedSimple(Current, "statement", "Expected a statement here.");
				SkipCurrent();
			}
		}

		return new(statements);
	}
	#endregion

	#region Statement methods
	private IConcreteToken ExpectStatementTerminator()
	{
		if (Match(SyntaxKind.Semicolon, ClassificationKind.Punctuation, out IConcreteToken? terminator))
			return terminator;

		terminator = Fabricate(SyntaxKind.Semicolon, ClassificationKind.Punctuation);

		IndexedPositionRange position = Previous?.Position ?? terminator.Position;
		ReportExpectedSimple(terminator, "terminator", "Expected a semi-colon '", SemicolonFragment, "' here to end the statement.");

		return terminator;
	}
	private IConcreteStatementSyntax ParseStatement()
	{
		if (TryParseStatement(out IConcreteStatementSyntax? statement))
			return statement;

		Debug.Assert(Current is not null, "EOF should still be there.");
		ReportExpectedSimple(Current, "statement", "Expected a statement here.");

		return new ConcreteEmptyStatementSyntax();
	}
	private bool TryParseStatement([NotNullWhen(true)] out IConcreteStatementSyntax? statement)
	{
		statement = TryParseStatement();
		return statement is not null;
	}
	private IConcreteStatementSyntax? TryParseStatement()
	{
		return
			TryParseOnlyTerminatedStatement() ??
			TryParseLocalFunctionDeclaration() ??
			TryParseIfStatement() ??
			TryParseWhileStatement() ??
			TryParseBlockStatement() ??
			TryParseVariableDeclaration() ??
			TryParseReturnStatement() ??
			TryParseExpressionStatement();
	}
	#endregion

	#region Statement variant methods
	private IConcreteStatementSyntax? TryParseOnlyTerminatedStatement()
	{
		if (Match(SyntaxKind.Semicolon, ClassificationKind.Punctuation, out IConcreteToken? terminator) is false)
			return null;

		Diagnostics
			.BuildSuggestion(this, "remove_empty_statement")
			.Add(terminator, lines =>
			{
				lines
					.AddLine("Remove the empty statement.")
					.AddLine("This statement only contains the terminator, it does nothing, and you probably included it by accident.");
			});

		return new ConcreteOnlyTerminatedStatementSyntax(terminator);
	}
	private IConcreteStatementSyntax? TryParseExpressionStatement()
	{
		if (TryParseExpression(out IConcreteExpressionSyntax? expression) is false)
			return null;


		IConcreteToken terminator = ExpectStatementTerminator();
		return new ConcreteExpressionStatementSyntax(expression, terminator);
	}
	private IConcreteStatementSyntax? TryParseVariableDeclaration()
	{
		// Todo(Nightowl): Currently this is the easiest approach, but parser needs to be able to distinguish handle the ambiguity better;
		if ((Current?.Kind == SyntaxKind.Identifier && Next?.Kind == SyntaxKind.Identifier) is false)
			return null;

		if (TryParseType(out IConcreteTypeSyntax? type) is false)
			return null;

		IConcreteToken name = Expect(SyntaxKind.Identifier, ClassificationKind.Variable, token => ReportExpectedSimple(token, "variable_name", "Expect the name of the new variable."));
		IConcreteToken assignment = Expect(SyntaxKind.EqualSign, ClassificationKind.Punctuation, token =>
			ReportExpectedSimple(token, "equal_sign", "Expect an equal sign '", EqualSignFragment, "' between the variable name and its value."));

		IConcreteExpressionSyntax value = ParseExpression();
		IConcreteToken terminator = ExpectStatementTerminator();

		return new ConcreteVariableDeclarationStatementSyntax(type, name, assignment, value, terminator);
	}
	private IConcreteStatementSyntax? TryParseBlockStatement()
	{
		if (Match(SyntaxKind.OpenBrace, ClassificationKind.Punctuation, out IConcreteToken? start) is false)
			return null;

		SyntaxList<IConcreteStatementSyntax> statements = ParseStatements(SyntaxKind.CloseBrace);
		IConcreteToken end = ExpectMatching(SyntaxKind.CloseBrace, ClassificationKind.Punctuation, start, token =>
		{
			ReportExpectedMatchingBrace(token, start, "end the block statement");
		});

		return new ConcreteBlockStatementSyntax(start, statements, end);
	}
	private IConcreteStatementSyntax? TryParseReturnStatement()
	{
		if (Match(SyntaxKind.Return, ClassificationKind.Keyword, out IConcreteToken? keyword) is false)
			return null;

		IConcreteToken terminator;
		if (Current?.Kind != SyntaxKind.Semicolon)
		{
			IConcreteExpressionSyntax value = ParseExpression();
			terminator = ExpectStatementTerminator();

			return new ConcreteValueReturnStatementSyntax(keyword, value, terminator);
		}

		terminator = ExpectStatementTerminator();
		return new ConcreteReturnStatementSyntax(keyword, terminator);
	}
	private IConcreteStatementSyntax? TryParseIfStatement()
	{
		if (Match(SyntaxKind.If, ClassificationKind.Keyword, out IConcreteToken? keyword) is false)
			return null;

		IConcreteToken start = Expect(SyntaxKind.OpenBracket, ClassificationKind.Punctuation, token => ReportExpectedOpeningBracket(token, "prefix the if statement condition"));
		IConcreteExpressionSyntax condition = ParseExpression();
		IConcreteToken end = ExpectMatching(SyntaxKind.CloseBracket, ClassificationKind.Punctuation, start, token => ReportExpectedMatchingBracket(token, start, "end the condition"));

		IConcreteStatementSyntax trueClause = ParseStatement();

		if (Match(SyntaxKind.Else, ClassificationKind.Keyword, out IConcreteToken? @else) is false)
			return new ConcreteIfStatementSyntax(keyword, start, condition, end, trueClause);

		IConcreteStatementSyntax falseClause = ParseStatement();
		return new ConcreteIfElseStatementSyntax(keyword, start, condition, end, trueClause, @else, falseClause);
	}
	private IConcreteStatementSyntax? TryParseWhileStatement()
	{
		if (Match(SyntaxKind.While, ClassificationKind.Keyword, out IConcreteToken? keyword) is false)
			return null;

		IConcreteToken start = Expect(SyntaxKind.OpenBracket, ClassificationKind.Punctuation, token => ReportExpectedOpeningBracket(token, "prefix the while statement condition"));
		IConcreteExpressionSyntax condition = ParseExpression();
		IConcreteToken end = ExpectMatching(SyntaxKind.CloseBracket, ClassificationKind.Punctuation, start, token => ReportExpectedMatchingBracket(token, start, "end the condition"));
		IConcreteStatementSyntax body = ParseStatement();

		return new ConcreteWhileStatementSyntax(keyword, start, condition, end, body);
	}
	#endregion

	#region Function declaration methods
	private IConcreteStatementSyntax? TryParseLocalFunctionDeclaration()
	{
		if (Match(SyntaxKind.Fun, ClassificationKind.Keyword, out IConcreteToken? keyword) is false)
			return null;

		IConcreteToken name = Expect(SyntaxKind.Identifier, ClassificationKind.Function, token => ReportExpectedSimple(token, "function_name", "Expected the function name"));
		IConcreteToken start = Expect(SyntaxKind.OpenBracket, ClassificationKind.Punctuation, token => ReportExpectedOpeningBracket(token, "start the parameter list"));

		ConcreteFunctionDeclarationStatementSyntax function = ParseFunctionDeclaration(keyword, name, start);
		return function;
	}
	private IConcreteStatementSyntax? TryParseFunctionDeclaration()
	{
		IConcreteStatementSyntax? local = TryParseLocalFunctionDeclaration();
		if (local is not null)
			return local;

		if (Current?.Kind == SyntaxKind.Identifier && Next?.Kind == SyntaxKind.OpenBracket)
		{
			IConcreteToken name = Convert(Current, ClassificationKind.Function);
			IConcreteToken start = Convert(Next, ClassificationKind.Punctuation);
			Advance(2);

			return ParseFunctionDeclaration(null, name, start);
		}

		return null;
	}
	private ConcreteFunctionDeclarationStatementSyntax ParseFunctionDeclaration(IConcreteToken? keyword, IConcreteToken name, IConcreteToken start)
	{
		IConcreteFunctionDeclarationSignatureSyntax signature = ParseFunctionSignature(keyword, name, start);
		IConcreteFunctionBodySyntax body = ParseFunctionBody();

		return new(signature, body);
	}
	private ConcreteFunctionDeclarationSignatureSyntax ParseFunctionSignature(IConcreteToken? keyword, IConcreteToken name, IConcreteToken start)
	{
		List<IConcreteSyntaxNode> nodes = [];
		List<IConcreteFunctionParameterSyntax> parameters = [];
		List<IConcreteToken> separators = [];

		while (RealisticHasRemaining && Current.Kind != SyntaxKind.CloseBracket)
		{
			if (IsCurrentAny(SyntaxKind.Semicolon, SyntaxKind.OpenBrace, SyntaxKind.EqualArrow)) // missing ')' but body started
				break;

			using LoopGuardScope _ = LoopGuard();

			IConcreteFunctionParameterSyntax? parameter = TryParseFunctionParameter();
			if (parameter is not null)
			{
				parameters.Add(parameter);
				nodes.Add(parameter);
			}
			else
				ReportExpectedSimple(Current, "function_parameter", "Expected a function parameter here.");

			if (RealisticHasRemaining && Current.Kind != SyntaxKind.CloseBracket)
			{
				if (Match(SyntaxKind.Comma, ClassificationKind.Punctuation, out IConcreteToken? comma))
				{
					nodes.Add(comma);
					separators.Add(comma);
				}
				else if (IsCurrentAny(SyntaxKind.Semicolon, SyntaxKind.OpenBrace, SyntaxKind.EqualArrow)) // missing ')' but body started
					break;
				else
					ReportExpectedComma(Current, "separate the function parameters");
			}
		}

		if (separators.Count > 0 && separators.Count >= parameters.Count)
			ReportExpectedSimple(separators[^1], "function_parameter", "Expected a function parameter here.");

		IConcreteToken end;
		if (IsCurrentAny(SyntaxKind.Semicolon, SyntaxKind.OpenBrace, SyntaxKind.EqualArrow))
		{
			// Note(Nightowl): I don't know if this is really needed, but I'm doing it this way for now;
			IConcreteToken target = nodes.LastOrDefault()?.Flatten().LastOrDefault() ?? start;
			ReportExpectedSimple(target, "closing_bracket", "Expected a closing bracket '", ClosingBracketFragment, "' here to end the function parameters.");
			end = Fabricate(SyntaxKind.Semicolon, ClassificationKind.Punctuation);
		}
		else
		{
			end = ExpectMatching(
				SyntaxKind.CloseBracket,
				ClassificationKind.Punctuation,
				start,
				token => ReportExpectedMatchingBracket(token, start, "end the function parameters"));
		}

		IConcreteFunctionReturnSyntax @return = ParseFunctionReturn();

		return new(
			keyword,
			name,
			start,
			new SyntaxList<IConcreteFunctionParameterSyntax, IConcreteToken>(nodes, parameters, separators),
			end,
			@return);
	}

	private IConcreteFunctionParameterSyntax? TryParseFunctionParameter()
	{
		return TryParseRegularFunctionParameter();
	}
	private IConcreteFunctionParameterSyntax? TryParseRegularFunctionParameter()
	{
		if (TryParseType(out IConcreteTypeSyntax? type) is false)
			return null;

		IConcreteToken name = Expect(SyntaxKind.Identifier, ClassificationKind.Parameter, token =>
		{
			ReportExpectedSimple(token, "function_parameter_name", "Expected the name of the function parameter.");
		});

		return new ConcreteRegularFunctionParameterSyntax(type, name);
	}
	private IConcreteFunctionReturnSyntax ParseFunctionReturn()
	{
		if (Match(SyntaxKind.Colon, ClassificationKind.Punctuation, out IConcreteToken? colon) is false)
			return new ConcreteEmptyFunctionReturnSyntax();

		IConcreteTypeSyntax type = ParseType();
		return new ConcreteRegularFunctionReturnSyntax(colon, type);
	}
	private IConcreteFunctionBodySyntax ParseFunctionBody()
	{
		if (TryParseFunctionBody(out IConcreteFunctionBodySyntax? body))
			return body;

		Debug.Assert(Current is not null, "EOF should still be here.");

		Diagnostics
			.BuildError(this, "expected_function_body")
			.Add(Current, lines => lines.AddLine("Expected a function body here."));

		return new ConcreteEmptyFunctionBodySyntax();
	}
	private bool TryParseFunctionBody([NotNullWhen(true)] out IConcreteFunctionBodySyntax? body)
	{
		body = TryParseFunctionBody();
		return body is not null;
	}
	private IConcreteFunctionBodySyntax? TryParseFunctionBody()
	{
		return
			TryParseTerminatedFunctionBody() ??
			TryParseShortFunctionBody() ??
			TryParseBlockFunctionBody();
	}
	private IConcreteFunctionBodySyntax? TryParseTerminatedFunctionBody()
	{
		if (Match(SyntaxKind.Semicolon, ClassificationKind.Punctuation, out IConcreteToken? token) is false)
			return null;

		return new ConcreteOnlyTerminatedFunctionBodySyntax(token);
	}
	private IConcreteFunctionBodySyntax? TryParseShortFunctionBody()
	{
		if (Match(SyntaxKind.EqualArrow, ClassificationKind.Punctuation, out IConcreteToken? arrow) is false)
			return null;

		IConcreteExpressionSyntax expression = ParseExpression();
		IConcreteToken terminator = Expect(SyntaxKind.Semicolon, ClassificationKind.Punctuation, token =>
		{
			Diagnostics
				.BuildError(this, "expected_terminator")
				.Add(token, lines => lines.AddLine("Expected a semi-colon '", SemicolonFragment, "' here to end the function body short-hand."))
				.Add(arrow, lines => lines.AddLine("This arrow here means that you started the function body short-hand."));
		});

		return new ConcreteShortFunctionBodySyntax(arrow, expression, terminator);
	}
	private IConcreteFunctionBodySyntax? TryParseBlockFunctionBody()
	{
		IConcreteBlockStatementSyntax? block = (IConcreteBlockStatementSyntax?)TryParseBlockStatement();
		if (block is null)
			return null;

		return new ConcreteBlockFunctionBodySyntax(block);
	}
	#endregion

	#region Type methods
	private IConcreteTypeSyntax ParseType()
	{
		if (TryParseType(out IConcreteTypeSyntax? type))
			return type;

		Debug.Assert(Current is not null, "EOF should still be there.");
		ReportExpectedSimple(Current, "type", "Expected a type here");

		return new ConcreteEmptyTypeSyntax();
	}
	private bool TryParseType([NotNullWhen(true)] out IConcreteTypeSyntax? type)
	{
		type = TryParseType();
		return type is not null;
	}
	private IConcreteTypeSyntax? TryParseType()
	{
		IConcreteTypeSyntax? type = TryParseRegularType();

		while (type is not null)
		{
			IConcreteTypeSyntax? newType = TryParseComplexType(type);
			if (newType is null)
				return type;

			type = newType;
		}

		return null;
	}
	private IConcreteTypeSyntax? TryParseComplexType(IConcreteTypeSyntax type)
	{
		return
			TryParseNestedType(type) ??
			TryParseGenericType(type);
	}
	private IConcreteTypeSyntax? TryParseGenericType(IConcreteTypeSyntax type)
	{
		if (type is IConcreteGenericTypeSyntax)
			return null;

		if (Match(SyntaxKind.OpenAngleBracket, ClassificationKind.Punctuation, out IConcreteToken? start) is false)
			return null;

		List<IConcreteSyntaxNode> nodes = [];
		List<IConcreteTypeSyntax> arguments = [];
		List<IConcreteToken> separators = [];

		while (RealisticHasRemaining && Current.Kind != SyntaxKind.CloseAngleBracket)
		{
			using LoopGuardScope _ = LoopGuard();

			IConcreteTypeSyntax? argument = TryParseType();
			if (argument is not null)
			{
				arguments.Add(argument);
				nodes.Add(argument);
			}
			else
			{
				Diagnostics
					.BuildError(this, "expected_generic_type_argument")
					.Add(Current, lines => lines.AddLine("Expected a type here to use as an argument to the generic type."));
			}

			if (RealisticHasRemaining && Current.Kind != SyntaxKind.CloseAngleBracket)
			{
				if (Match(SyntaxKind.Comma, ClassificationKind.Punctuation, out IConcreteToken? comma) is false)
					break;

				nodes.Add(comma);
				separators.Add(comma);
			}
		}

		IConcreteToken end = Expect(SyntaxKind.CloseAngleBracket, ClassificationKind.Punctuation, token =>
		{
			Diagnostics
				.BuildError(this, "expected_generic_type_end")
				.Add(token, lines => lines.AddLine("Expected a closing angle bracket '", ClosingAngleBracketFragment, "' to end the generic type."))
				.Add(start, lines => lines.AddLine("It needs to match this opening angle bracket '", OpeningAngleBracketFragment, "'."));
		});

		return new ConcreteGenericTypeSyntax(
			type,
			start,
			new SyntaxList<IConcreteTypeSyntax, IConcreteToken>(nodes, arguments, separators),
			end);
	}
	private IConcreteTypeSyntax? TryParseNestedType(IConcreteTypeSyntax type)
	{
		return null;

		// Note(Nightowl): Nested types are not yet a thing because the parser is ambiguous for them;

		/*
			if (Match(SyntaxKind.Period, ClassificationKind.Punctuation, out IConcreteToken? period) is false)
				return null;

			IConcreteToken name = Expect(SyntaxKind.Identifier, ClassificationKind.Type, token => ReportExpectedSimple(token, "type_name", "Expected the name of the nested type here."));

			return new ConcreteNestedTypeSyntax(type, period, name);
		*/
	}
	private IConcreteTypeSyntax? TryParseRegularType()
	{
		if (Match(SyntaxKind.Identifier, ClassificationKind.Type, out IConcreteToken? type) is false)
			return null;

		return new ConcreteRegularTypeSyntax(type);
	}
	#endregion

	#region Expression methods
	private IConcreteExpressionSyntax ParseExpression(ExpressionPower precedence = default)
	{
		if (TryParseExpression(out IConcreteExpressionSyntax? expression, precedence))
			return expression;

		Debug.Assert(Current is not null, "EOF should still be there.");
		ReportExpectedSimple(Current, "expression", "Expected an expression here.");

		return new ConcreteEmptyExpressionSyntax();
	}
	private bool TryParseExpression([NotNullWhen(true)] out IConcreteExpressionSyntax? expression, ExpressionPower precedence = default)
	{
		expression = TryParseExpression(precedence);
		return expression is not null;
	}
	private IConcreteExpressionSyntax? TryParseExpression(ExpressionPower precedence = default)
	{
		IConcreteExpressionSyntax? literal = TryParseLiteral();

		if (literal is null)
			return null;

		return ParseExpression(literal, precedence);
	}
	private IConcreteExpressionSyntax ParseExpression(IConcreteExpressionSyntax expression, ExpressionPower precedence = default)
	{
		while (RealisticHasRemaining)
		{
			while (true)
			{
				ExpressionPower power = ExpressionPower.PowerOf(Current.Kind);

				if (precedence.Value >= power.Value)
					break;

				if (Match(SyntaxKind.OpenBracket, ClassificationKind.Punctuation, out IConcreteToken? openBracket))
					expression = ParseFunctionCallExpression(expression, openBracket);
				else if (Match(SyntaxKind.Period, ClassificationKind.Punctuation, out IConcreteToken? dot))
					expression = ParseMemberAccess(expression, dot);
				else if (MatchAny(out IConcreteToken? @operator, ClassificationKind.Operator, SyntaxKind.BinaryOperators))
				{
					IConcreteExpressionSyntax right = ParseExpression(power);

					if (@operator.Kind == SyntaxKind.EqualSign)
						expression = new ConcreteAssignmentExpressionSyntax(expression, @operator, right);
					else if (@operator.Kind.IsCompoundAssignmentOperator())
						expression = new ConcreteCompoundAssignmentExpressionSyntax(expression, @operator, right);
					else
						expression = new ConcreteBinaryExpressionSyntax(expression, @operator, right);
				}
			}

			return expression;
		}

		return expression;
	}
	private IConcreteExpressionSyntax? TryParseLiteral()
	{
		return
			TryParseBooleanLiteral() ??
			TryParseNumberLiteral() ??
			TryParseString() ??
			TryParseInterpolatedString() ??
			TryParseGroupedExpression() ??
			TryParseGetExpression();
	}
	private IConcreteExpressionSyntax? TryParseGroupedExpression()
	{
		if (Match(SyntaxKind.OpenBracket, ClassificationKind.Punctuation, out IConcreteToken? start) is false)
			return null;

		IConcreteExpressionSyntax expression = ParseExpression();
		IConcreteToken end = ExpectMatching(SyntaxKind.CloseBracket, ClassificationKind.Punctuation, start, token => ReportExpectedMatchingBracket(token, start, "end the grouped expression"));

		return new ConcreteGroupedExpressionSyntax(start, expression, end);
	}
	private IConcreteExpressionSyntax? TryParseGetExpression()
	{
		if (Match(SyntaxKind.Identifier, ClassificationKind.Identifier, out IConcreteToken? name) is false)
			return null;

		return new ConcreteGetExpressionSyntax(name);
	}
	private IConcreteExpressionSyntax ParseMemberAccess(IConcreteExpressionSyntax expression, IConcreteToken dot)
	{
		IConcreteToken name = Expect(SyntaxKind.Identifier, ClassificationKind.Identifier, token => ReportExpectedSimple(token, "member_name", "Expected the name of member you're trying to access."));
		return new ConcreteMemberAccessExpressionSyntax(expression, dot, name);
	}
	#endregion

	#region Function call expression methods
	private ConcreteFunctionCallExpressionSyntax ParseFunctionCallExpression(IConcreteExpressionSyntax expression, IConcreteToken start)
	{
		List<IConcreteSyntaxNode> nodes = [];
		List<IConcreteFunctionArgumentSyntax> arguments = [];
		List<IConcreteToken> separators = [];

		IConcreteToken? lastComma = null;
		while (RealisticHasRemaining && Current.Kind != SyntaxKind.CloseBracket)
		{
			using LoopGuardScope _ = LoopGuard();

			IConcreteFunctionArgumentSyntax? argument = TryParseFunctionArgument();
			if (argument is not null)
			{
				arguments.Add(argument);
				nodes.Add(argument);
			}
			else
			{
				Diagnostic diagnostic = Diagnostics
					.BuildError(this, "expected_function_argument")
					.Add(Source, Current.Position, lines =>
					{
						lines.AddLine("Expected a function argument.");
						if (lastComma is null && Current.Kind != SyntaxKind.CloseBracket)
							lines.AddLine("I don't know why the parsing failed here, I'd appreciate it if you could let me know this happened.");
					})
					.Add(start, lines =>
					{
						lines.AddLine(
							"This opening bracket '",
							OpeningBracketFragment,
							"' is used to call a function. You end this call with a closing bracket '",
							ClosingBracketFragment,
							"'.");
					});

				if (lastComma is not null)
				{
					diagnostic.Add(lastComma, lines =>
					{
						lines
						.AddLine("This is a comma '", CommaFragment, "', in this context, it is used to separate the function arguments.")
						.AddLine("Writing it here means that you intended to pass in another argument to the function.");
					});
				}

				SkipCurrent();
			}

			if (RealisticHasRemaining && Current.Kind != SyntaxKind.CloseBracket)
			{
				if (Match(SyntaxKind.Comma, ClassificationKind.Punctuation, out IConcreteToken? comma) is false)
					break;

				nodes.Add(comma);
				separators.Add(comma);

				lastComma = comma;
			}
		}

		IConcreteFunctionArgumentSyntax? lastNonNamed = arguments.LastOrDefault(a => a is not IConcreteNamedFunctionArgumentSyntax);
		IEnumerable<IConcreteNamedFunctionArgumentSyntax>? mispositionedNamed = lastNonNamed is null ? [] :
			arguments
			.OfType<IConcreteNamedFunctionArgumentSyntax>()
			.Where(a => a.Position.Start < lastNonNamed.Position.Start);

		if (mispositionedNamed.Any())
		{
			Debug.Assert(lastNonNamed is not null);
			Diagnostic diagnostic = Diagnostics.BuildError(this, "named_argument_not_at_end");

			diagnostic.Add(mispositionedNamed.First(), lines => lines.AddLine("Named arguments should come after positioned ones."));
			foreach (IConcreteNamedFunctionArgumentSyntax argument in mispositionedNamed.Skip(1))
				diagnostic.Add(argument.Name, lines => lines.AddLine("Also wrong."));

			IndexedLinePosition endOfLast = lastNonNamed.Position.End;
			IndexedLinePosition afterLast = new(endOfLast.Index + 1, endOfLast.Line, endOfLast.Column + 1);
			diagnostic.Add(Source, afterLast, lines => lines.AddLine("Move them here."));
		}

		IConcreteToken end = Expect(SyntaxKind.CloseBracket, ClassificationKind.Punctuation, token => ReportExpectedMatchingBracket(token, start, "End the function call"));

		return new(
			expression,
			start,
			new SyntaxList<IConcreteFunctionArgumentSyntax, IConcreteToken>(nodes, arguments, separators),
			end);
	}
	private IConcreteFunctionArgumentSyntax? TryParseFunctionArgument()
	{
		return
			TryParseNamedFunctionArgument() ??
			TryParseRegularFunctionArgument();
	}
	private IConcreteFunctionArgumentSyntax? TryParseRegularFunctionArgument()
	{
		if (TryParseExpression(out IConcreteExpressionSyntax? expression))
			return new ConcreteRegularFunctionArgumentSyntax(expression);

		return null;
	}
	private IConcreteFunctionArgumentSyntax? TryParseNamedFunctionArgument()
	{
		if (Current?.Kind == SyntaxKind.Identifier && Next?.Kind == SyntaxKind.Colon)
		{
			IConcreteToken name = Convert(Current, ClassificationKind.Parameter);
			IConcreteToken colon = Convert(Next, ClassificationKind.Punctuation);
			Advance(2);

			IConcreteExpressionSyntax expression = ParseExpression();
			return new ConcreteNamedFunctionArgumentSyntax(name, colon, expression);
		}

		return null;
	}
	#endregion

	#region Boolean expression methods
	private IConcreteExpressionSyntax? TryParseBooleanLiteral()
	{
		if (Match(SyntaxKind.Boolean, ClassificationKind.Boolean, out IConcreteToken? token) is false)
			return null;

		return new ConcreteBooleanLiteralExpressionSyntax(token, (bool?)token.Value);
	}
	#endregion

	#region Number expression methods
	private IConcreteExpressionSyntax? TryParseNumberLiteral()
	{
		if (Match(SyntaxKind.IntegerBase, ClassificationKind.Number, out IConcreteToken? @base))
		{
			Debug.Assert(@base.Value is not null);
			NumberBase numberBase = (NumberBase)@base.Value;
			IConcreteToken basedInteger = Expect(SyntaxKind.Integer, ClassificationKind.Number, token =>
			{
				Diagnostics
					.BuildError(this, "expected_integer_literal")
					.Add(token, lines => lines.AddLine("Expected an integer literal after the base specifier."))
					.Add(@base, lines =>
					{
						lines.AddLine(
							"This is a number base specifier, this particular one is for ",
							(numberBase.Name.ToLower(), ClassificationKind.Number),
							$" digits, meaning you can only use the {numberBase.CharacterSetDisplay} characters, or an underscore '",
							NumberUnderscoreFragment,
							"' to separate the digits.");
					});

			});

			return new ConcreteBaseIntegerLiteralExpressionSyntax(@base, basedInteger, (ulong?)basedInteger.Value);
		}

		if (Match(SyntaxKind.Integer, ClassificationKind.Number, out IConcreteToken? integer) is false)
			return null;

		if (Current?.Kind == SyntaxKind.Period && Next?.Kind == SyntaxKind.Integer)
		{
			IConcreteToken period = Convert(Current, ClassificationKind.Number);
			IConcreteToken @decimal = Convert(Next, ClassificationKind.Number);
			Advance(2);

			string valueText = $"{(integer.Value as long?) ?? 0}.{@decimal.Value as long? ?? 0}";
			decimal? value;

			if (decimal.TryParse(valueText, out decimal v) is false)
			{
				value = default;
				Diagnostics
					.BuildError(this, "invalid_decimal_literal")
					.Add(integer, lines => lines.AddLine("Couldn't convert the number literal to a decimal value, it is likely too precise. I would appreciate it if you'd let me know what number didn't work."));
			}
			else
				value = v;

			return new ConcreteDecimalLiteralExpressionSyntax(integer, period, @decimal, value);
		}

		return new ConcreteIntegerLiteralExpressionSyntax(integer, (long?)integer.Value);
	}
	#endregion

	#region String expression methods
	private IConcreteExpressionSyntax? TryParseString()
	{
		if (Match(SyntaxKind.StringStart, ClassificationKind.String, out IConcreteToken? start) is false)
			return null;

		List<IConcreteStringFragmentSyntax> fragments = [];
		StringBuilder builder = new();

		while (RealisticHasRemaining && Current.Kind != SyntaxKind.StringEnd)
		{
			IConcreteStringFragmentSyntax? fragment = TryParseBasicStringFragments(out string? value);
			if (fragment is null)
				break; // Unclosed string, error reported by lexer.

			fragments.Add(fragment);
			builder.Append(value);
		}

		IConcreteToken end = ExpectSilent(SyntaxKind.StringEnd, ClassificationKind.String); // Diagnostic reported by lexer already.

		return new ConcreteStringLiteralExpressionSyntax(
			start,
			new SyntaxList<IConcreteStringFragmentSyntax>(fragments),
			end,
			builder.ToString());
	}
	private IConcreteExpressionSyntax? TryParseInterpolatedString()
	{
		if (Match(SyntaxKind.InterpolatedStringStart, ClassificationKind.String, out IConcreteToken? start) is false)
			return null;

		List<IConcreteStringFragmentSyntax> fragments = [];

		while (RealisticHasRemaining && Current.Kind != SyntaxKind.StringEnd)
		{
			IConcreteStringFragmentSyntax? fragment = TryParseBasicStringFragments(out string? _) ?? TryParseStringInterpolation();
			if (fragment is null)
				break; // Unclosed string, error reported by lexer.

			fragments.Add(fragment);
		}

		IConcreteToken end = ExpectSilent(SyntaxKind.StringEnd, ClassificationKind.String); // Diagnostic reported by lexer already.

		return new ConcreteInterpolatedStringExpressionSyntax(
			start,
			new SyntaxList<IConcreteStringFragmentSyntax>(fragments),
			end);
	}
	private IConcreteStringFragmentSyntax? TryParseBasicStringFragments(out string? value)
	{
		value = Current?.Value as string;

		if (Match(SyntaxKind.StringText, ClassificationKind.String, out IConcreteToken? text))
			return new ConcreteRegularStringFragmentSyntax(text);

		if (Match(SyntaxKind.StringHexSequence, ClassificationKind.StringEscape, out IConcreteToken? hex))
			return new ConcreteEscapedHexStringFragmentSyntax(hex);

		if (Match(SyntaxKind.StringEscape, ClassificationKind.StringEscape, out IConcreteToken? escape))
			return new ConcreteEscapedStringFragmentSyntax(escape);

		return null;
	}
	private IConcreteStringFragmentSyntax? TryParseStringInterpolation()
	{
		if (Match(SyntaxKind.StringInterpolationStart, ClassificationKind.Punctuation, out IConcreteToken? start) is false)
			return null;

		IConcreteExpressionSyntax expression = ParseExpression();
		IConcreteToken end = ExpectSilent(SyntaxKind.StringInterpolationEnd, ClassificationKind.Punctuation); // error reported by lexer.

		return new ConcreteInterpolatedStringFragmentSyntax(start, expression, end);
	}
	#endregion

	#region Parsing helpers
	private bool Match(SyntaxKind kind, [NotNullWhen(true)] out IConcreteToken? token)
	{
		if (Match(kind, out ISyntaxToken? untyped))
		{
			token = Convert(untyped);
			return true;
		}

		token = default;
		return false;
	}
	private bool Match(SyntaxKind kind, ClassificationKind classification, [NotNullWhen(true)] out IConcreteToken? token)
	{
		if (Match(kind, out ISyntaxToken? untyped))
		{
			token = Convert(untyped, classification);
			return true;
		}

		token = default;
		return false;
	}
	private bool MatchAny([NotNullWhen(true)] out IConcreteToken? token, ClassificationKind classification, params ReadOnlySpan<SyntaxKind> kinds)
	{
		if (MatchAny(out ISyntaxToken? untyped, kinds))
		{
			token = Convert(untyped, classification);
			return true;
		}

		token = default;
		return false;
	}
	private bool MatchAny([NotNullWhen(true)] out IConcreteToken? token, ClassificationKind classification, params IReadOnlyCollection<SyntaxKind> kinds)
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

	#region Error recovery methods
	private IConcreteToken ExpectSilent(SyntaxKind kind)
	{
		ISyntaxToken token = ExpectSilentCore(kind);
		return Convert(token);
	}
	private IConcreteToken ExpectSilent(SyntaxKind kind, ClassificationKind classification)
	{
		ISyntaxToken token = ExpectSilentCore(kind);
		return Convert(token, classification);
	}
	private IConcreteToken Expect(SyntaxKind kind, ClassificationKind classification, Action<IConcreteToken> callback)
	{
		if (Match(kind, classification, out IConcreteToken? token))
			return token;

		token = Fabricate(kind, classification);
		callback.Invoke(token);

		return token;
	}
	private IConcreteToken ExpectMatching(SyntaxKind kind, ClassificationKind classification, ISyntaxToken start, Action<IConcreteToken> callback)
	{
		if (Match(kind, classification, out IConcreteToken? end) is false)
		{
			end = Fabricate(kind, classification);
			callback.Invoke(end);
		}

		return end;
	}
	private IConcreteToken Expect(SyntaxKind kind, Action<ISyntaxToken> message)
	{
		ISyntaxToken token = ExpectCore(kind, message);
		return Convert(token);
	}

	private IConcreteToken Fabricate(SyntaxKind kind)
	{
		ISyntaxToken token = FabricateCore(kind);
		return Convert(token);
	}
	private IConcreteToken Fabricate(SyntaxKind kind, ClassificationKind classification)
	{
		ISyntaxToken token = FabricateCore(kind);
		return Convert(token, classification);
	}

	[return: NotNullIfNotNull(nameof(token))]
	private IConcreteToken? Convert(ISyntaxToken? token, ClassificationKind? classification = null)
	{
		if (token is null)
			return null;

		classification ??= TryEstimateClassification(token);

		ConcreteToken newToken = new(
			token.Kind,
			token.Position,
			token.Lexeme,
			token.Value,
			token.LeadingTrivia,
			token.TrailingTrivia,
			token.IsFabricated,
			classification);

		token.ShadowedBy = newToken;
		return newToken;
	}
	private ClassificationKind? TryEstimateClassification(ISyntaxToken token)
	{
		if (IsKeyword(token.Kind))
			return ClassificationKind.Keyword;

		if (IsStringEscape(token.Kind))
			return ClassificationKind.StringEscape;

		if (IsString(token.Kind))
			return ClassificationKind.String;

		if (IsPunctuation(token.Kind))
			return ClassificationKind.Punctuation;

		if (token.Kind == SyntaxKind.Identifier)
			return ClassificationKind.Identifier;

		return null;
	}
	private bool IsKeyword(SyntaxKind kind)
	{
		foreach (SyntaxKind current in SyntaxKind.AllKeywords)
		{
			if (kind == current)
				return true;
		}

		return false;
	}
	private bool IsStringEscape(SyntaxKind kind)
	{
		return
			kind == SyntaxKind.StringEscape ||
			kind == SyntaxKind.StringHexSequence;
	}
	private bool IsString(SyntaxKind kind)
	{
		return
			kind == SyntaxKind.StringStart ||
			kind == SyntaxKind.InterpolatedStringStart ||
			kind == SyntaxKind.StringEnd ||
			kind == SyntaxKind.StringText ||
			kind == SyntaxKind.StringEscape ||
			kind == SyntaxKind.StringHexSequence;
		;
	}
	private bool IsPunctuation(SyntaxKind kind)
	{
		return
			kind == SyntaxKind.Semicolon ||
			kind == SyntaxKind.Colon ||
			kind == SyntaxKind.Comma ||
			kind == SyntaxKind.Period ||
			kind == SyntaxKind.QuestionMark ||

			kind == SyntaxKind.Plus ||
			kind == SyntaxKind.Minus ||
			kind == SyntaxKind.Divide ||
			kind == SyntaxKind.Star ||
			kind == SyntaxKind.Modulo ||

			kind == SyntaxKind.PlusEqual ||
			kind == SyntaxKind.MinusEqual ||
			kind == SyntaxKind.DivideEqual ||
			kind == SyntaxKind.StarEqual ||
			kind == SyntaxKind.ModuloEqual ||

			kind == SyntaxKind.DoubleEqualSign ||
			kind == SyntaxKind.NotEqual ||
			kind == SyntaxKind.LessThanOrEqual ||
			kind == SyntaxKind.GreaterThanOrEqual ||

			kind == SyntaxKind.DoubleAmpersand ||
			kind == SyntaxKind.DoublePipe ||

			kind == SyntaxKind.EqualSign ||
			kind == SyntaxKind.OpenBrace ||
			kind == SyntaxKind.CloseBrace ||
			kind == SyntaxKind.OpenBracket ||
			kind == SyntaxKind.CloseBracket ||
			kind == SyntaxKind.OpenSquareBracket ||
			kind == SyntaxKind.CloseSquareBracket ||
			kind == SyntaxKind.OpenAngleBracket ||
			kind == SyntaxKind.CloseAngleBracket ||
			kind == SyntaxKind.StringInterpolationStart ||
			kind == SyntaxKind.StringInterpolationEnd
		;
	}
	#endregion

	#region Diagnostic methods
	private void ReportDuplicate(ISyntaxToken token, string kind, TextFragment fragment)
	{
		Diagnostics
			.BuildError(this, $"duplicate_{kind}")
			.Add(token, lines => lines.AddLine($"A duplicate {kind.Replace('_', ' ')} '", fragment, "' was encountered here."));
	}
	private void ReportExpectedSimple(ISyntaxToken fabricatedToken, string kind, params IEnumerable<object?> message)
	{
		Diagnostics
			.BuildError(this, $"expected_{kind}")
			.Add(fabricatedToken, lines => lines.AddLine(message));
	}
	private void ReportExpectedOpeningBracket(ISyntaxToken fabricatedToken, string purpose)
	{
		Diagnostics
			.BuildError(this, $"expected_bracket")
			.Add(fabricatedToken, lines => lines.AddLine("Expected an opening bracket '", OpeningBracketFragment, $"' here to {purpose}."));
	}
	private void ReportExpectedComma(ISyntaxToken fabricatedToken, string purpose)
	{
		Diagnostics
			.BuildError(this, $"expected_comma")
			.Add(fabricatedToken, lines => lines.AddLine("Expected a comma '", CommaFragment, $"' here to {purpose}."));
	}
	private void ReportExpectedMatchingBracket(ISyntaxToken fabricated, ISyntaxToken start, string purpose)
	{
		Diagnostics
				.BuildError(this, "expected_closing_bracket")
				.Add(fabricated, lines => lines.AddLine("Expected a closing bracket '", ClosingBracketFragment, $"' to {purpose}."))
				.Add(start, lines => lines.AddLine("It needs to match this opening bracket '", OpeningBracketFragment, "'."));
	}
	private void ReportExpectedMatchingAngleBracket(ISyntaxToken fabricated, ISyntaxToken start, string purpose)
	{
		Diagnostics
				.BuildError(this, "expected_closing_angle_bracket")
				.Add(fabricated, lines => lines.AddLine("Expected a closing angle bracket '", ClosingAngleBracketFragment, $"' to {purpose}."))
				.Add(start, lines => lines.AddLine("It needs to match this opening angle bracket '", OpeningAngleBracketFragment, "'."));
	}
	private void ReportExpectedMatchingBrace(ISyntaxToken fabricated, ISyntaxToken start, string purpose)
	{
		Diagnostics
				.BuildError(this, "expected_closing_brace")
				.Add(fabricated, lines => lines.AddLine("Expected a closing brace '", ClosingBraceFragment, $"' to {purpose}."))
				.Add(start, lines => lines.AddLine("It needs to match this opening brace '", OpeningBraceFragment, "'."));
	}


	protected override void ReportInfiniteLoop(ISyntaxToken token)
	{
		StackTrace trace = new();

		Diagnostics
			.BuildError(this, "infinite_parsing_loop", trace)
			.Add(token, lines => lines.AddLine("The parser got stuck in an infinite loop without me accounting for it. I'd appreciate it if you told me that it happened."));
	}
	#endregion
}
