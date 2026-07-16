using System.IO;
using OwlDomain.Owl.Code.Execution;
using OwlDomain.Owl.Code.Execution.Builtins;
using Spectre.Console.Rendering;

namespace OwlDomain.Owl.CLI.Actions.Run;

public class GeneralRunCommand : Command
{
	#region Fields
	#endregion

	#region Constructors
	public GeneralRunCommand() : base("run", "Lets you run OWL code.")
	{
		Argument<string> pathArgument = new("path")
		{
			HelpName = "path",
			Description = "The path of the file to run.",
		};

		Add(pathArgument);

		SetAction(parsing =>
		{
			string path = parsing.GetRequiredValue(pathArgument);
			string? file = GetFile(path);

			if (file is null)
			{
				parsing.CommandResult.AddError($"The given file '{path}' was not found.");
				return -1;
			}

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


			var tree = context.Bundles.Single().MostDetailed;
			Debug.Assert(tree is not null);


			InterpretingResult interpreting = Interpreter.Interpret(tree);
			if (interpreting.Diagnostics.HasErrors)
			{
				Explain(context, interpreting);
				PrintDiagnosticCounts(interpreting.Diagnostics);

				return -1;
			}

			return 0;
		});
	}
	#endregion

	#region Helpers
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
		if (diagnostics.Count is 0)
			return;

		TextFragmentCollection fragments = [];
		fragments.Add("Encountered", ClassificationKind.Message);

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
		AnsiConsole.WriteLine();
	}
	private static string? GetFile(string path)
	{
		List<string> attempts = [path];

		if (Path.HasExtension(path) is false)
			attempts.Add(Path.ChangeExtension(path, ".owl"));

		foreach (string attempt in attempts)
		{
			if (File.Exists(attempt))
				return attempt;
		}

		return null;
	}
	#endregion
}
