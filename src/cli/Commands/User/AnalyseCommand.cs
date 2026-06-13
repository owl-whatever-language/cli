namespace OwlDomain.Owl.CLI.Commands.User;

[DisplayName("analyse")]
[Description("Analyses the given source input.")]
public sealed class AnalyseCommand : Command<AnalyseCommand.Settings>
{
	#region Nested types
	public sealed class Settings : CommandSettings, IPerformanceReportSettings
	{
		#region Properties
		[CommandOption("-e|--example", IsHidden = true)]
		[Description("The example to analyse (internal use only)")]
		public string? Example { get; init; }

		[CommandArgument(0, "[input]")]
		[Description("The source input to analyse.")]
		public string? Input { get; init; }

		[CommandOption("--report-performance")]
		[Description("Whether to create a report of the compiler's performance during the requested operation")]
		[DefaultValue(false)]
		public bool ReportPerformance { get; init; }
		#endregion
	}
	#endregion

	#region Methods
	protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
	{
		if (settings.Input is not null && settings.Example is not null)
			return Error("The input source and the example cannot be specified at the same time.");

		string? input = settings.Input;
		if (settings.Example is not null)
		{
			string? examples = GetExampleRoot();
			if (examples is null)
				return Error("The examples directory could not be found.");

			string example = Path.Combine(examples, settings.Example);
			if (File.Exists(example) is false)
			{
				example += ".owl";
				if (File.Exists(example) is false)
					return Error("The example file could not be found.");
			}

			input = example;
		}

		// Note(Nightowl): These validations should;
		if (File.Exists(input) is false)
			return Error("The given input '{input}' was not a file.");

		if (Path.GetExtension(input) != ".owl")
			return Error("The given input '{input}' was not an OWL source file.");

		if (settings.Example is not null)
			AnsiConsole.WriteLine($"Analysing example: {Path.GetFileName(input)}");
		else
		{
			AnsiConsole.Write("Analysing source file: ");
			AnsiConsole.Write(new TextPath(Path.GetRelativePath(Environment.CurrentDirectory, input)));
			AnsiConsole.WriteLine();
		}

		List<IStageResult> results = [];
		FinalSyntaxTree? tree = null;

		void Analyse(StatusContext status)
		{
			Lexer lexer = new();
			Parser parser = new();
			SymbolFinder finder = new();
			SemanticResolver resolver = new();
			SyntaxFinaliser finaliser = new();

			FileSystemSourceFile file = new(input);

			status.Status("Lexing the file into tokens...");
			ILexerResult lexerResult = lexer.Lex(file);
			results.Add(lexerResult);

			status.Status("Parsing the tokens into a concrete syntax tree (CST)...");
			ParserResult parserResult = parser.Parse(lexerResult);
			results.Add(parserResult);

			status.Status("Discovering symbols...");
			ISymbolScope rootSymbols = CreateRootSymbolScope();
			ISymbolDiscoveryResult discoveryResult = finder.Explore(rootSymbols, [parserResult.Tree]);
			results.Add(discoveryResult);

			status.Status("Resolving semantics...");
			SemanticResolutionInput resolverInputs = new(discoveryResult.Symbols, discoveryResult.Targets);
			SemanticResolutionResult resolverResult = resolver.Resolve(resolverInputs, parserResult.Tree);
			results.Add(resolverResult);

			status.Status("Finalising syntax...");
			SyntaxFinalisationResult finalisationResult = finaliser.Finalise(resolverResult.Tree);
			results.Add(finalisationResult);
			tree = finalisationResult.Tree;
		}

		AnsiConsole.Status().Start("Preparing for analysis...", Analyse);

		int width = Math.Min(120, AnsiConsole.Profile.Width);

		IDiagnostic[] diagnostics = results.SelectMany(r => r.Diagnostics).OrderBy(d => d.Kind.Name).ToArray();

		if (tree is not null)
			DisplaySource(width, tree, diagnostics);

		if (diagnostics.Length > 0)
			DisplayDiagnostics(width, diagnostics);
		else
			AnsiConsole.WriteLine("Analysing successful, no diagnostics to report.");

		if (settings.ReportPerformance)
			DisplayPerformanceReport(width, results);

		return diagnostics.Length is 0 ? 0 : -1;
	}

	private static void DisplaySource(int width, FinalSyntaxTree tree, IReadOnlyCollection<IDiagnostic> diagnostics)
	{
		IReadOnlyList<IFinalSyntaxToken> tokens = tree.Document.Flatten();

		List<string> lines = [];
		List<AnsiMarkupSegment> currentLine = [];

		IDiagnostic? GetDiagnosticForLine(int line)
		{
			IDiagnostic? highest = null;
			int highestLevel = 0;

			foreach (IDiagnostic diagnostic in diagnostics)
			{
				if (diagnostic.Location is not DiagnosticSourceLocation location)
					continue;

				if (location.Source != tree.Source)
					continue;

				if (location.Position?.Start.Line == line)
				{
					if (highest is null || diagnostic.Kind.Level > highestLevel)
					{
						highest = diagnostic;
						highestLevel = diagnostic.Kind.Level;
					}
				}
			}

			return highest;
		}
		void CompleteLine()
		{
			IDiagnostic? diagnostic = GetDiagnosticForLine(lines.Count + 1);

			if (diagnostic is not null)
			{
				Style style = SemanticColors.GetStyle(diagnostic.Kind);
				currentLine.Add(new($" // {diagnostic.Message}", style, null));
			}

			string total = string.Concat(currentLine);
			currentLine.Clear();

			lines.Add(total);
		}
		void AddTrivia(TriviaList list)
		{
			foreach (ITriviaNode trivia in list)
			{
				if (trivia.Kind == SyntaxKind.LineBreak)
				{
					CompleteLine();
					continue;
				}

				if (trivia.Lexeme is null)
					continue;

				Style style = SemanticColors.GetStyle(trivia);
				currentLine.Add(new(trivia.Lexeme, style, null));
			}
		}

		foreach (IFinalSyntaxToken token in tokens)
		{
			AddTrivia(token.LeadingTrivia);

			if (token.Lexeme is not null)
			{
				Style style = SemanticColors.GetStyle(token);
				currentLine.Add(new(token.Lexeme, style, null));
			}

			AddTrivia(token.TrailingTrivia);
		}

		if (currentLine.Count > 0)
			CompleteLine();

		while (lines.Count > 0 && lines.Last().IsWhiteSpace())
			lines.RemoveAt(lines.Count - 1);

		int maxLineNumberLength = lines.Count.ToString("n0").Length;
		List<Markup> markupLines = [];

		for (int i = 0; i < lines.Count; i++)
		{
			string line = $"[gray] {(i + 1).ToString("n0").PadLeft(maxLineNumberLength)} | [/]" + lines[i];
			Markup markup = new(line);
			markupLines.Add(markup);
		}

		Panel panel = new Panel(new Rows(markupLines))
			.Header($"[bold] Source ({tree.Source.SimpleName:n0}) [/]", Justify.Center);

		panel.Width = width;

		AnsiConsole.Write(panel);
	}
	private static void DisplayDiagnostics(int width, IReadOnlyList<IDiagnostic> diagnostics)
	{
		Table table = new Table()
			.Border(TableBorder.MinimalHeavyHead)
			.ShowRowSeparators()
			.AddColumns("Severity", "Provider", "Message", "File", "Position");

		foreach (IDiagnostic diagnostic in diagnostics.OrderByDescending(d => d.Kind.Level))
		{
			string file = "", position = "";
			Style style = SemanticColors.GetStyle(diagnostic.Kind);

			if (diagnostic.Location is DiagnosticSourceLocation location)
			{
				file = location.Source.SimpleName;
				position = location.Position?.ToString() ?? "";
			}

			table.AddRow(
				new Markup(diagnostic.Kind.Name, style),
				new Text(diagnostic.Provider.Name),
				new Text(diagnostic.Message),
				new Text(file),
				new Text(position));
		}

		Panel panel = new Panel(table)
			.Header($"[bold] Diagnostics ({diagnostics.Count:n0}) [/]", Justify.Center);

		panel.Width = width;


		AnsiConsole.Write(panel);
	}
	private static void DisplayPerformanceReport(int width, IReadOnlyCollection<IStageResult> results)
	{
		List<Color> colors = [Color.Blue, Color.Green, Color.Yellow, Color.Red, Color.Purple];
		Dictionary<string, Color> usedColors = [];

		BarChart bar = new BarChart()
			.UseValueFormatter(value => $"{value:n3}s");

		BreakdownChart breakdown = new BreakdownChart()
			.HideTags()
			.UseValueFormatter(value => $"{value:n3}s");

		int colorIndex = 0;
		TimeSpan totalDuration = TimeSpan.FromSeconds(results.Sum(r => r.Duration.TotalSeconds));
		foreach (IGrouping<string, IStageResult> group in results.GroupBy(result => result.Name))
		{
			Color color = colors[colorIndex];
			double total = group.Sum(g => g.Duration.TotalSeconds);

			bar.AddItem(group.Key, total, color);
			breakdown.AddItem(group.Key, total, color);

			colorIndex = (colorIndex + 1) % colors.Count;
		}

		Rows rows = new(
			Text.Empty, bar,
			Text.Empty, breakdown);

		Panel panel = new Panel(rows)
			.Header($"[bold] Stage performance ({totalDuration.TotalSeconds:n3}s) [/]", Justify.Center);

		panel.Width = width;

		AnsiConsole.Write(panel);
	}
	private static ISymbolScope CreateRootSymbolScope()
	{
		SymbolScope root = new("root");
		SymbolScope builtin = new("builtin", root);
		builtin.Add(SpecialTypes.Text.Symbol);

		FunctionInfo printFunction =
			new FunctionInfo(
				new("print",
					new FunctionParameterSignature(SpecialTypes.Text, "value")))
			.WithSymbol("print")
			.Locked();

		builtin.Add(printFunction.Symbol);

		return builtin;
	}
	private static int Error(string error)
	{
		AnsiConsole.WriteLine($"[red]{error.EscapeMarkup()}[/]");
		return -1;
	}
	private static string? GetExampleRoot()
	{
		string? project = GetProjectRoot();
		if (project is null)
			return project;

		string examples = Path.Combine(project, "examples");
		if (Directory.Exists(examples))
			return examples;

		return null;
	}
	private static string? GetProjectRoot()
	{
		string? directory = Environment.CurrentDirectory;
		while (directory is not null)
		{
			if (Directory.EnumerateFiles(directory, "*.slnx").Any())
				return directory;

			directory = Path.GetDirectoryName(directory);
		}

		return null;
	}
	#endregion
}
