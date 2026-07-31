namespace OwlDomain.Owl.Packages.CodeAnalysis.Parsing;

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
		RecoverUntilEndOfInput();
		IConcreteToken endOfInput = ExpectEndOfInput();

		return new(endOfInput);
	}
	#endregion

	#region Error recovery methods
	protected override ISyntaxNode? TryParseBadSyntaxCore() => null;

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
