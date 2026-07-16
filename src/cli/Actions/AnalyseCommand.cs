using System.IO;
using OwlDomain.Owl.Code.Execution.Builtins;
using Spectre.Console.Rendering;

namespace OwlDomain.Owl.CLI.Actions;

public class AnalyseCommand : Command
{
	#region Constructors
	public AnalyseCommand() : base("analyse", "Lets you analyse OWL code.")
	{
		Aliases.Add("analyze");

		Argument<string> pathArgument = new("path")
		{
			HelpName = "path",
			Description = "The path of the file to analyse.",
		};

		Option<bool> watchOption = new("--watch", "-w")
		{
			Description = "Enables watch mode that will re-analyse the file when it changes."
		};

		Add(pathArgument);
		Add(watchOption);

		SetAction(parsing =>
		{
			string path = parsing.GetRequiredValue(pathArgument);
			bool watch = parsing.GetValue(watchOption);

			if (watch)
			{
				AnsiConsole.AlternateScreen(() => Watch(path));
				return 0;
			}

			if (File.Exists(path) is false)
			{
				parsing.CommandResult.AddError($"The given file '{path}' was not found.");
				return -1;
			}

			return Analyse(path);
		});
	}
	#endregion

	#region Helpers
	private static void Watch(string path)
	{
		ManualResetEventSlim reset = new(true);

		string fullPath = Path.GetFullPath(path);

		FileSystemWatcher watcher = new(Path.GetDirectoryName(fullPath)!, Path.GetFileName(fullPath));
		const WatcherChangeTypes changes = WatcherChangeTypes.All;

		while (true)
		{
			Console.Clear();

			if (File.Exists(fullPath) is false)
			{
				AnsiConsole.MarkupLineInterpolated($"[red italic]File '{path}' didn't exist.[/]");
				AnsiConsole.MarkupLineInterpolated($"[grey italic]Watching file '{path}' for changes.[/]");
				watcher.WaitForChanged(changes);
				continue;
			}

			Analyse(fullPath);
			AnsiConsole.MarkupLineInterpolated($"[grey italic]Watching file '{path}' for changes.[/]");
			watcher.WaitForChanged(changes);
		}
	}
	private static int Analyse(string file)
	{
		FileSystemSourceFile source = new(file);

		BuiltinResolutionResult builtinResult = BuiltinResolver.Resolve();
		AnalysisContext context = new(builtinResult.ResultScope);
		AnalysisUpdateResult analysis = context.Update(added: [source]);

		List<IStageResult> results = [builtinResult, analysis];
		Explain(context, results);

		IDiagnosticBag allDiagnostics = results.GetAllDiagnostics();
		PrintDiagnosticCounts(allDiagnostics);

		if (allDiagnostics.HasErrors)
			return -1;

		return 0;
	}
	private static void Explain(IAnalysisContext context, params IReadOnlyCollection<IStageResult> results)
	{
		IRenderable? explanations = DiagnosticExplainer.Explain(context, results);
		if (explanations is not null)
		{
			AnsiConsole.Write(explanations);
			AnsiConsole.WriteLine();
		}
	}
	private static void PrintDiagnosticCounts(IDiagnosticBag diagnostics)
	{
		TextFragmentCollection fragments = [];
		fragments.Add("Encountered", ClassificationKind.Message);

		if (diagnostics.Count is 0)
			fragments.Add(" no problems.", ClassificationKind.Message);

		foreach (var group in diagnostics.GroupBy(d => d.Kind).OrderByDescending(g => g.Key.Level))
		{
			fragments.Add(" ", ClassificationKind.Whitespace);

			int count = group.Count();
			if (count is 1)
				fragments.Add($"1 {group.Key.Name}", group.Key.ToClassification());
			else
			{
				// Note(Nightowl): Naive but it'll work for now;
				fragments.Add($"{count} {group.Key.Name}s", group.Key.ToClassification());
			}
		}

		AnsiConsole.Write(fragments.StyleMarkup(OwlStyling.Default));
		AnsiConsole.WriteLine();
	}
	#endregion
}
