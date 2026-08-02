namespace OwlDomain.Owl.Config.CodeAnalysis.Parsing;

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

public sealed class Parser : BaseParser<IConcreteToken>
{
	#region Constructors
	private Parser(ISourceFile source, IReadOnlyList<ISyntaxToken> tokens) : base(source, tokens) { }
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
	private ConcreteDocumentSyntax ParseDocument()
	{
		IConcreteDocumentUnitSyntax unit = ParseDocumentUnit();

		RecoverUntilEndOfInput();
		IConcreteToken endOfInput = ExpectEndOfInput();

		return new(unit, endOfInput);
	}
	#endregion

	#region Document unit methods
	private IConcreteDocumentUnitSyntax ParseDocumentUnit()
	{
		if (Source.IsPackageFile)
			return ParsePackageDocument();

		if (Source.IsWorkspaceFile)
			return ParseWorkspaceDocument();

		return ParseConfigDocument();
	}
	private ConcretePackageDocumentUnitSyntax ParsePackageDocument()
	{
		var statements = ParseDocumentStatements();
		return new(statements);
	}
	private ConcreteWorkspaceDocumentUnitSyntax ParseWorkspaceDocument()
	{
		var statements = ParseDocumentStatements();
		return new(statements);
	}
	private ConcreteConfigDocumentUnitSyntax ParseConfigDocument()
	{
		var statements = ParseDocumentStatements();
		return new(statements);
	}
	#endregion

	#region Statement methods
	private SyntaxList<IConcreteStatementSyntax> ParseDocumentStatements()
	{
		SyntaxList<IConcreteStatementSyntax> statements = ParseStatements();

		Debug.Assert(Current is not null);
		if (Current.Kind != SyntaxKind.EndOfInput)
			ReportExpectedSimple(Current, "statement", "Expected a statement here.");

		return statements;
	}
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

				ReportDuplicate(target);
				RecoverFromCurrent();
			}
			else
			{
				Debug.Assert(Current is not null, "EOF should still be here.");
				ReportExpectedSimple(Current, "statement", "Expected a statement here.");
				RecoverFromCurrent();
			}
		}

		return new(statements);
	}
	private IConcreteToken ExpectStatementTerminator(IConcreteSyntaxNode? value)
	{
		if (Match(SyntaxKind.Semicolon, ClassificationKind.Punctuation, out IConcreteToken? terminator))
			return terminator;

		terminator = Fabricate(SyntaxKind.Semicolon, ClassificationKind.Punctuation);

		ISyntaxToken token =
			Previous ??
			value?.Flatten()?.LastOrDefault() ??
			terminator;

		Diagnostics
			.BuildError(this, "expected_terminator")
			.Add(token, token.Position.End, lines => lines.AddLine("Expected a semi-colon '", TextFragment.Semicolon, "' here to end the statement."));

		return terminator;
	}
	private IConcreteToken? ExpectOptionalStatementTerminator(IConcreteSyntaxNode value)
	{
		if (Match(SyntaxKind.Semicolon, ClassificationKind.Punctuation, out IConcreteToken? terminator))
			return terminator;

		ISyntaxToken token = Previous ?? value.Flatten().Last();
		if (token.TrailingTrivia.Any(t => t.Kind == SyntaxKind.LineBreak) || Current?.Kind == SyntaxKind.EndOfInput)
			return null;

		terminator = Fabricate(SyntaxKind.Semicolon, ClassificationKind.Punctuation);

		Diagnostics
			.BuildError(this, "expected_terminator")
			.Add(token, token.Position.End, lines => lines.AddLine("Expected either a line break, or a semi-colon '", TextFragment.Semicolon, "' here to end the statement."));

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
			TryParseKeywordStatement() ??
			TryParsePropertyStatement()
		;
	}
	private IConcreteStatementSyntax? TryParseKeywordStatement()
	{
		return
			null
		;
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
	private IConcreteStatementSyntax? TryParsePropertyStatement()
	{
		if (TryParsePropertyKey(out IConcretePropertyKeySyntax? key) is false)
			return null;

		if (Match(SyntaxKind.OpenBrace, ClassificationKind.Punctuation, out IConcreteToken? start))
			return ParsePropertyScopeStatement(key, start);

		if (Current?.Kind != SyntaxKind.Colon)
			return ParseImplicitPropertyStatement(key);

		IConcreteToken separator = Expect(SyntaxKind.Colon, ClassificationKind.Punctuation, ":", "separate the property key from the value");
		IConcretePropertyValueSyntax value = ParsePropertyValue();
		IConcreteToken? terminator = ExpectOptionalStatementTerminator(value);

		return new ConcretePropertyStatementSyntax(key, separator, value, terminator);
	}
	private IConcreteStatementSyntax ParseImplicitPropertyStatement(IConcretePropertyKeySyntax key)
	{
		IConcreteToken? terminator = ExpectOptionalStatementTerminator(key);
		return new ConcreteImplicitPropertyStatementSyntax(key, terminator);
	}
	private IConcreteStatementSyntax ParsePropertyScopeStatement(IConcretePropertyKeySyntax key, IConcreteToken start)
	{
		SyntaxList<IConcreteStatementSyntax> statements = ParseStatements(SyntaxKind.CloseBrace);
		IConcreteToken end = ExpectClosing(start, SyntaxKind.CloseBrace, ClassificationKind.Punctuation, "}", "end the property scope");

		return new ConcretePropertyScopeStatementSyntax(key, start, statements, end);
	}
	#endregion

	#region Property key methods
	private IConcretePropertyKeySyntax ParsePropertyKey(ExpressionPower precedence = default)
	{
		if (TryParsePropertyKey(out IConcretePropertyKeySyntax? key, precedence))
			return key;

		Debug.Assert(Current is not null, "EOF should still be there.");
		ReportExpectedSimple(Current, "property_key", "Expected a property key here.");

		return new ConcreteEmptyPropertyKeySyntax();
	}
	private bool TryParsePropertyKey([NotNullWhen(true)] out IConcretePropertyKeySyntax? key, ExpressionPower precedence = default)
	{
		key = TryParsePropertyKey(precedence);
		return key is not null;
	}
	private IConcretePropertyKeySyntax? TryParsePropertyKey(ExpressionPower precedence = default)
	{
		IConcretePropertyKeySyntax? primitive = TryParsePrimitivePropertyKey();

		if (primitive is null)
			return null;

		return ParsePropertyKey(primitive, precedence);
	}
	private IConcretePropertyKeySyntax ParsePropertyKey(IConcretePropertyKeySyntax key, ExpressionPower precedence = default)
	{
		// Note(Nightowl): This was changed from the primary parser as I'm not sure why the original parser was written the way that it is...;

		while (RealisticHasRemaining)
		{
			ExpressionPower power = ExpressionPower.PropertyKeyPowerOf(Current.Kind);

			if (precedence.Value >= power.Value)
				break;

			if (Match(SyntaxKind.Period, ClassificationKind.Punctuation, out IConcreteToken? accessor))
			{
				IConcreteToken name = Expect(SyntaxKind.Identifier, ClassificationKind.Key, "Expected a key name.");
				key = new ConcreteNestedPropertyKeySyntax(key, accessor, name);
			}
		}

		return key;
	}
	private IConcretePropertyKeySyntax? TryParsePrimitivePropertyKey()
	{
		return
			TryParseNamedPropertyKey()
		;
	}
	private IConcretePropertyKeySyntax? TryParseNamedPropertyKey()
	{
		if (Match(SyntaxKind.Identifier, ClassificationKind.Key, out IConcreteToken? name) is false)
			return null;

		return new ConcreteNamedPropertyKeySyntax(name);
	}
	#endregion

	#region Property value methods
	private IConcretePropertyValueSyntax ParsePropertyValue(ExpressionPower precedence = default)
	{
		if (TryParsePropertyValue(out IConcretePropertyValueSyntax? value, precedence))
			return value;

		Debug.Assert(Current is not null, "EOF should still be there.");
		ReportExpectedSimple(Current, "property_value", "Expected a property value here.");

		return new ConcreteEmptyPropertyValueSyntax();
	}
	private bool TryParsePropertyValue([NotNullWhen(true)] out IConcretePropertyValueSyntax? value, ExpressionPower precedence = default)
	{
		value = TryParsePropertyValue(precedence);
		return value is not null;
	}
	private IConcretePropertyValueSyntax? TryParsePropertyValue(ExpressionPower precedence = default)
	{
		IConcretePropertyValueSyntax? primitive = TryParsePrimitivePropertyValue();

		if (primitive is null)
			return null;

		return ParsePropertyValue(primitive, precedence);
	}
	private IConcretePropertyValueSyntax ParsePropertyValue(IConcretePropertyValueSyntax value, ExpressionPower precedence = default)
	{
		// Note(Nightowl): This was changed from the primary parser as I'm not sure why the original parser was written the way that it is...;

		while (RealisticHasRemaining)
		{
			ExpressionPower power = ExpressionPower.PropertyValuePowerOf(Current.Kind);

			if (precedence.Value >= power.Value)
				break;

			if (Match(SyntaxKind.Period, ClassificationKind.Punctuation, out IConcreteToken? accessor))
			{
				IConcreteToken name = Expect(SyntaxKind.Identifier, ClassificationKind.Value, "Expected a value name.");
				value = new ConcreteNestedPropertyValueSyntax(value, accessor, name);
			}
		}

		return value;
	}
	private IConcretePropertyValueSyntax? TryParsePrimitivePropertyValue()
	{
		return
			TryParseNamedPropertyValue()
		;
	}
	private IConcretePropertyValueSyntax? TryParseNamedPropertyValue()
	{
		if (Match(SyntaxKind.Identifier, ClassificationKind.Value, out IConcreteToken? name) is false)
			return null;

		return new ConcreteNamedPropertyValueSyntax(name);
	}
	#endregion

	#region Error recovery methods
	protected override ISyntaxNode? TryParseBadSyntaxCore()
	{
		return
			TryParseKeywordStatement()
		;
	}

	[return: NotNullIfNotNull(nameof(token))]
	protected override IConcreteToken? Convert(ISyntaxToken? token, ClassificationKind? classification = null)
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
			kind == SyntaxKind.CloseAngleBracket
		;
	}
	#endregion
}
