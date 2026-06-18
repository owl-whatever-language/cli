using System.Text;

namespace OwlDomain.Owl.Code.CodeAnalysis.Parsing;

public sealed class ParsingResult : SourceStageResult
{
	#region Properties
	public override string Stage => "parsing";
	public IConcreteSyntaxTree Tree { get; }
	#endregion

	#region Constructors
	public ParsingResult(
		IDiagnosticBag diagnostics,
		IPerformanceResult performance,
		ISourceFile source,
		IConcreteSyntaxTree tree)
		: base(diagnostics, performance, source)
	{
		Tree = tree;
	}
	#endregion
}

public sealed class LexingAndParsingResult : SourceStageResult
{
	#region Properties
	public override string Stage => "lexing_and_parsing";
	public LexingResult Lexing { get; }
	public ParsingResult Parsing { get; }
	#endregion

	#region Constructors
	public LexingAndParsingResult(
		IPerformanceResult performance,
		LexingResult lexing,
		ParsingResult parsing)
		: base(new DiagnosticBag(), performance, lexing.Source, [lexing, parsing])
	{
		Lexing = lexing;
		Parsing = parsing;
	}
	#endregion
}

public sealed class ParallelParsingResult : StageResult
{
	#region Properties
	public override string Stage => "parallel_parsing";
	public IReadOnlyDictionary<ISourceFile, LexingAndParsingResult> ByFile { get; }
	#endregion

	#region Constructors
	public ParallelParsingResult(
		IPerformanceResult performance,
		IReadOnlyList<LexingAndParsingResult> results) : base(new DiagnosticBag(), performance, results, ResultKind.Parallel)
	{
		ByFile = results.ToDictionary(r => r.Source);
	}
	#endregion

	#region Methods
	public IReadOnlyDictionary<string, IPerformanceResult> GetStageBreakdown()
	{
		// Note(Nightowl):
		// I have absolutely no idea how mathematically sound this approach is for getting an estimate of parallelised results.
		// The approach that I'm taking here is to calculate the total values as if the result wasn't parallelised
		// in order to get a % share of the performance for a particular stage, and then I use that % on the true
		// parallelised performance result.

		IEnumerable<IGrouping<string, IStageResult>> stages = SubResults.SelectMany(r => r.SubResults).GroupBy(s => s.Stage);
		Dictionary<string, IPerformanceResult> totals = [];

		long totalMemory = 0;
		TimeSpan totalSystem = default, totalUser = default;

		foreach (IGrouping<string, IStageResult> group in stages)
		{
			long memory = 0;
			TimeSpan system = default, user = default;

			foreach (IStageResult result in group)
			{
				memory += result.Performance.MemoryUsed;
				system += result.Performance.SystemTime;
				user += result.Performance.UserTime;
			}

			totalMemory += memory;
			totalSystem += system;
			totalUser += user;

			PerformanceResult total = new(system, user, system + user, memory);
			totals.Add(group.Key, total);
		}

		TimeSpan totalDuration = totalSystem + totalUser;

		Dictionary<string, IPerformanceResult> breakdowns = [];
		foreach (KeyValuePair<string, IPerformanceResult> pair in totals)
		{
			double systemShare = pair.Value.SystemTime / totalSystem;
			double userShare = pair.Value.UserTime / totalUser;
			double durationShare = pair.Value.Duration / totalDuration;
			double memoryShare = pair.Value.MemoryUsed / (double)totalMemory;

			TimeSpan system = systemShare * Performance.SystemTime;
			TimeSpan user = userShare * Performance.UserTime;
			TimeSpan duration = durationShare * Performance.Duration;
			long memory = (long)(memoryShare * Performance.MemoryUsed);

			PerformanceResult estimate = new(system, user, duration, memory);
			breakdowns.Add(pair.Key, pair.Value);
		}

		return breakdowns;
	}
	#endregion
}

public sealed class Parser : BaseParser, IDiagnosticProvider
{
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

			return new(parser.Diagnostics, performance, source, tree);
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
			ReportExpectedStatement(Current.Position);

		SkipToEndOfInput();
		IConcreteToken endOfInput = Expect(SyntaxKind.EndOfInput, "Expected the end of the input.");

		return new(statements, endOfInput);
	}
	private SyntaxList<IConcreteStatementSyntax> ParseDocumentStatements() => ParseStatements();
	private SyntaxList<IConcreteStatementSyntax> ParseStatements() => ParseStatements(() => RealisticHasRemaining, TryParseStatement);
	private SyntaxList<IConcreteStatementSyntax> ParseStatements(Func<IConcreteStatementSyntax?> parselet)
	{
		return ParseStatements(() => RealisticHasRemaining, parselet);
	}
	private SyntaxList<IConcreteStatementSyntax> ParseStatements(Func<bool> condition)
	{
		return ParseStatements(condition, TryParseStatement);
	}
	private SyntaxList<IConcreteStatementSyntax> ParseStatements(Func<bool> condition, Func<IConcreteStatementSyntax?> parselet)
	{
		List<IConcreteStatementSyntax> statements = [];

		void Body()
		{
			IConcreteStatementSyntax? statement = parselet.Invoke();
			if (statement is not null)
			{
				if (statement.IsFabricated is false)
					statements.Add(statement);
			}
			else
			{
				Debug.Assert(Current is not null, "EOF should still be here.");
				ReportExpectedStatement(Current.Position);
				SkipCurrent();
			}
		}

		LoopGuard(condition, Body);

		return new(statements);
	}
	#endregion

	#region Statement methods
	private IConcreteToken ExpectStatementTerminator() => Expect(SyntaxKind.Semicolon, ClassificationKind.Punctuation, "Expected a semi-colon ';' to end the statement.");
	private IConcreteStatementSyntax ParseStatement()
	{
		if (TryParseStatement(out IConcreteStatementSyntax? statement))
			return statement;

		Debug.Assert(Current is not null, "EOF should still be there.");
		ReportExpectedStatement(Current.Position);

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
			TryParseBlockStatement() ??
			TryParseVariableDeclaration() ??
			TryParseExpressionStatement();
	}
	private IConcreteStatementSyntax? TryParseOnlyTerminatedStatement()
	{
		if (Match(SyntaxKind.Semicolon, ClassificationKind.Punctuation, out IConcreteToken? terminator) is false)
			return null;

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
		if (Current?.Kind == SyntaxKind.Identifier && Next?.Kind == SyntaxKind.OpenBracket)
			return TryParseExpressionStatement();

		if (TryParseType(out IConcreteTypeSyntax? type) is false)
			return null;

		IConcreteToken name = Expect(SyntaxKind.Identifier, ClassificationKind.Variable, "Expect the name of the new variable.");
		IConcreteToken assignment = Expect(SyntaxKind.EqualSign, ClassificationKind.Punctuation, "Expect an equal sign '=' between the variable name and its value.");
		IConcreteExpressionSyntax value = ParseExpression();
		IConcreteToken terminator = ExpectStatementTerminator();

		return new ConcreteVariableDeclarationStatementSyntax(type, name, assignment, value, terminator);
	}
	private IConcreteStatementSyntax? TryParseBlockStatement()
	{
		if (Match(SyntaxKind.OpenBrace, ClassificationKind.Punctuation, out IConcreteToken? start) is false)
			return null;

		SyntaxList<IConcreteStatementSyntax> statements = ParseStatements(() => HasRemaining && Current.Kind != SyntaxKind.CloseBrace);
		IConcreteToken end = Expect(SyntaxKind.CloseBrace, ClassificationKind.Punctuation, "Expected a closing brace '}' to end the statement block.");

		return new ConcreteBlockStatementSyntax(start, statements, end);
	}
	#endregion

	#region Function declaration methods
	private IConcreteStatementSyntax? TryParseLocalFunctionDeclaration()
	{
		if (Match(SyntaxKind.Fun, ClassificationKind.Keyword, out IConcreteToken? keyword) is false)
			return null;

		IConcreteToken name = Expect(SyntaxKind.Identifier, ClassificationKind.Function, "Expected the function name.");
		IConcreteToken start = Expect(SyntaxKind.OpenBracket, ClassificationKind.Punctuation, "Expected the opening bracket '(' to start the parameter list.");

		ConcreteFunctionDeclarationStatementSyntax function = ParseFunctionDeclaration(name, start);
		return new ConcreteLocalFunctionDeclarationStatementSyntax(keyword, function);
	}
	private IConcreteStatementSyntax? TryParseFunctionDeclaration()
	{
		if (Current?.Kind == SyntaxKind.Identifier && Next?.Kind == SyntaxKind.OpenBracket)
		{
			IConcreteToken name = Convert(Current, ClassificationKind.Function);
			IConcreteToken start = Convert(Next, ClassificationKind.Punctuation);
			Advance(2);

			return ParseFunctionDeclaration(name, start);
		}

		return null;
	}
	private ConcreteFunctionDeclarationStatementSyntax ParseFunctionDeclaration(IConcreteToken name, IConcreteToken start)
	{
		List<IConcreteSyntaxNode> nodes = [];
		List<IConcreteFunctionParameterSyntax> parameters = [];
		List<IConcreteToken> separators = [];

		void Body()
		{
			Debug.Assert(Current is not null);

			IConcreteFunctionParameterSyntax? parameter = TryParseFunctionParameter();
			if (parameter is not null)
			{
				parameters.Add(parameter);
				nodes.Add(parameter);
			}
			else
				ReportExpectedFunctionArgument(Current.Position);

			if (RealisticHasRemaining && Current.Kind != SyntaxKind.CloseBracket)
			{
				if (Match(SyntaxKind.Comma, ClassificationKind.Punctuation, out IConcreteToken? comma))
				{
					nodes.Add(comma);
					separators.Add(comma);
				}
				else
					ReportExpectedFunctionParameterSeparator(Current.Position);
			}
		}

		LoopGuard(() => RealisticHasRemaining && Current.Kind != SyntaxKind.CloseBracket, Body);
		IConcreteToken end = Expect(SyntaxKind.CloseBracket, ClassificationKind.Punctuation, "Expecting a closing bracket ')' to end the function parameters.");
		IConcreteFunctionReturnSyntax @return = ParseFunctionReturn();
		IConcreteFunctionBodySyntax body = ParseFunctionBody();

		return new(
			name,
			start,
			new SyntaxList<IConcreteFunctionParameterSyntax, IConcreteToken>(nodes, parameters, separators),
			end,
			@return,
			body);
	}
	private IConcreteFunctionParameterSyntax? TryParseFunctionParameter()
	{
		return TryParseRegularFunctionParameter();
	}
	private IConcreteFunctionParameterSyntax? TryParseRegularFunctionParameter()
	{
		if (TryParseType(out IConcreteTypeSyntax? type) is false)
			return null;

		IConcreteToken name = Expect(SyntaxKind.Identifier, ClassificationKind.Variable, "Expected the function parameter name.");
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
		ReportExpectedFunctionBody(Current.Position);

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
		IConcreteToken terminator = Expect(SyntaxKind.Semicolon, ClassificationKind.Punctuation, "Expected a semi-colon ';' to end the short function body.");

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
		ReportExpectedType(Current.Position);

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
				ReportExpectedType(Current.Position);

			if (RealisticHasRemaining && Current.Kind != SyntaxKind.CloseAngleBracket)
			{
				if (Match(SyntaxKind.Comma, ClassificationKind.Punctuation, out IConcreteToken? comma) is false)
					break;

				nodes.Add(comma);
				separators.Add(comma);
			}
		}

		IConcreteToken end = Expect(SyntaxKind.CloseAngleBracket, ClassificationKind.Punctuation, "Expecting a closing angle bracket '>' to end a generic type.");

		return new ConcreteGenericTypeSyntax(
			type,
			start,
			new SyntaxList<IConcreteTypeSyntax, IConcreteToken>(nodes, arguments, separators),
			end);
	}
	private IConcreteTypeSyntax? TryParseNestedType(IConcreteTypeSyntax type)
	{
		if (Match(SyntaxKind.Period, ClassificationKind.Punctuation, out IConcreteToken? period) is false)
			return null;

		IConcreteToken name = Expect(SyntaxKind.Identifier, ClassificationKind.Type, "Expected a type name.");

		return new ConcreteNestedTypeSyntax(type, period, name);
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
		ReportExpectedExpression(Current.Position);

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
			ExpressionPower power = ExpressionPower.PowerOf(Current.Kind);

			if (precedence.Value >= power.Value)
				break;

			if (Match(SyntaxKind.OpenBracket, ClassificationKind.Punctuation, out IConcreteToken? openBracket))
				expression = ParseFunctionCallExpression(expression, openBracket);

			return expression;
		}

		return expression;
	}
	private IConcreteExpressionSyntax? TryParseLiteral()
	{
		return
			TryParseString() ??
			TryParseInterpolatedString() ??
			TryParseGetExpression();
	}
	private IConcreteExpressionSyntax? TryParseGetExpression()
	{
		if (Match(SyntaxKind.Identifier, ClassificationKind.Identifier, out IConcreteToken? name) is false)
			return null;

		return new ConcreteGetExpressionSyntax(name);
	}
	#endregion

	#region Function call expression methods
	private ConcreteFunctionCallExpressionSyntax ParseFunctionCallExpression(IConcreteExpressionSyntax expression, IConcreteToken start)
	{
		List<IConcreteSyntaxNode> nodes = [];
		List<IConcreteFunctionArgumentSyntax> arguments = [];
		List<IConcreteToken> separators = [];

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
				ReportExpectedFunctionArgument(Current.Position);
				SkipCurrent();
			}

			if (RealisticHasRemaining && Current.Kind != SyntaxKind.CloseBracket)
			{
				if (Match(SyntaxKind.Comma, ClassificationKind.Punctuation, out IConcreteToken? comma) is false)
					break;

				nodes.Add(comma);
				separators.Add(comma);
			}
		}

		IConcreteToken end = Expect(SyntaxKind.CloseBracket, ClassificationKind.Punctuation, "Expecting a closing bracket ')' to end the function call.");

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
			IConcreteToken name = Convert(Current, ClassificationKind.Identifier);
			IConcreteToken colon = Convert(Next, ClassificationKind.Punctuation);
			Advance(2);

			IConcreteExpressionSyntax expression = ParseExpression();
			return new ConcreteNamedFunctionArgumentSyntax(name, colon, expression);
		}

		return null;
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
	private IConcreteToken Expect(SyntaxKind kind, ClassificationKind classification, string message)
	{
		ISyntaxToken token = ExpectCore(kind, message);
		return Convert(token, classification);
	}
	private IConcreteToken Expect(SyntaxKind kind, string message)
	{
		ISyntaxToken token = ExpectCore(kind, message);
		return Convert(token);
	}

	[return: NotNullIfNotNull(nameof(token))]
	private IConcreteToken? Convert(ISyntaxToken? token, ClassificationKind? classification = null)
	{
		if (token is null)
			return null;

		classification ??= TryEstimateClassification(token);

		return new ConcreteToken(token.Kind, token.Position, token.Lexeme, token.Value, token.LeadingTrivia, token.TrailingTrivia, classification);
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
			kind == SyntaxKind.EqualSign ||
			kind == SyntaxKind.DoubleEqualSign ||
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
	protected override void ReportExpectedToken(IndexedPositionRange position, SyntaxKind kind, string message)
	{
		AddError("expected_token", position, message);
	}
	protected override void ReportInfiniteLoop(IndexedPositionRange position)
	{
		StackTrace trace = new();
		AddError(
			"infinite_parsing_loop",
			position,
			"An unaccounted for infinite loop occurred during parsing, this is likely an error with the OWL parser. Feel free to scold me.",
			trace);
	}
	private void ReportExpectedStatement(IndexedPositionRange position)
	{
		AddError("expected_statement", position, "Expected a statement.");
	}
	private void ReportExpectedExpression(IndexedPositionRange position)
	{
		AddError("expected_expression", position, "Expected an expression.");
	}
	private void ReportExpectedFunctionArgument(IndexedPositionRange position)
	{
		AddError("expected_function_argument", position, "Expected a function argument.");
	}
	private void ReportExpectedFunctionArgumentSeparator(IndexedPositionRange position)
	{
		AddError("expected_function_argument_separator", position, "Expect a comma ',' to separate the function arguments.");
	}
	private void ReportExpectedType(IndexedPositionRange position)
	{
		AddError("expected_type", position, "Expected a type name.");
	}
	private void ReportExpectedFunctionParameter(IndexedPositionRange position)
	{
		AddError("expected_function_parameter", position, "Expected a function parameter.");
	}
	private void ReportExpectedFunctionParameterSeparator(IndexedPositionRange position)
	{
		AddError("expected_function_parameter_separator", position, "Expect a comma ',' to separate the function parameters.");
	}
	private void ReportExpectedFunctionBody(IndexedPositionRange position)
	{
		AddError("expected_function_body", position, "Expected the body of the function, or a semi-colon ';' to end the function declaration.");
	}
	#endregion

	#region Helpers
	private void AddError(string id, IndexedPositionRange position, string message, StackTrace? stackTrace = null)
	{
		AddDiagnostic(DiagnosticKind.Error, id, position, message, stackTrace);
	}
	private void AddDiagnostic(DiagnosticKind kind, string id, IndexedPositionRange position, string message, StackTrace? stackTrace = null)
	{
		Diagnostics.Add(new Diagnostic()
		{
			Provider = this,
			Kind = kind,
			Id = id,
			StackTrace = stackTrace,

			Location = new DiagnosticSourceLocation(Source, position),
			Message = message
		});
	}
	#endregion
}
