using System.IO;
using OwlDomain.Owl.Code.CodeAnalysis.Annotation.Flags;
using OwlDomain.Owl.Code.Execution;
using OwlDomain.Owl.Code.Execution.Builtins;
using OwlDomain.ParsingTools.Syntax.Printing;
using Spectre.Console.Rendering;

namespace OwlDomain.Owl.CLI.Actions.Run;

public class RunExample : Command
{
	#region Constructors
	public RunExample() : base("example", "Runs the specified example.")
	{
		Argument<string> nameArgument = new("name")
		{
			HelpName = "example_name",
			Description = "The name of the example file.",
			DefaultValueFactory = _ => "hoot_hoot.owl"
		};

		Add(nameArgument);

		SetAction(parsing =>
		{
			string? name = parsing.GetValue(nameArgument) ?? nameArgument.GetDefaultValue() as string;
			Debug.Assert(name is not null);

			string directory = Path.Combine(AppContext.BaseDirectory, "examples");

			if (Directory.Exists(directory) is false)
			{
				parsing.CommandResult.AddError("The example directory didn't exist.");
				return;
			}

			string? file = GetExampleFile(directory, name);
			if (file is null)
			{
				parsing.CommandResult.AddError($"No example named '{name}' could be found.");
				return;
			}

			FileSystemSourceFile source = new(file);

			BuiltinResolutionResult builtinResult = BuiltinResolver.Resolve();
			AnalysisContext context = new(builtinResult.ResultScope);
			AnalysisUpdateResult analysis = context.Update(added: [source]);

			List<IStageResult> results = [builtinResult, analysis];

			var tree = context.Bundles.Single().MostDetailed;
			Debug.Assert(tree is not null);

			var sourceLines = tree
				.GetLines()
				.TrimLastLines()
				.MarkUnreachable()
				.PrefixLineMargin();

			Rows output = sourceLines.Style(OwlStyling.Default);
			Panel panel = new Panel(new Padder(output)).Header(tree.Source.SimpleName);
			AnsiConsole.Write(panel);
			AnsiConsole.WriteLine();

			IRenderable? diagnosticExplanations = DiagnosticExplainer.Explain(context, results);
			if (diagnosticExplanations is not null)
			{
				AnsiConsole.Write(diagnosticExplanations);
				AnsiConsole.WriteLine();
			}

			IDiagnosticBag allDiagnostics = results.GetAllDiagnostics();
			PrintDiagnosticCounts(allDiagnostics);

			if (allDiagnostics.Count is 0)
			{
				AnsiConsole.MarkupLineInterpolated($"[grey italic]// Starting to interpret: {tree.Source.SimpleName}[/]");
				Console.WriteLine();

				InterpretingResult interpretingResult = Interpreter.Interpret(tree);

				Console.WriteLine();
				AnsiConsole.MarkupLine($"[grey italic]// Interpretation finished[/]");
				Console.WriteLine();
			}
		});
	}
	#endregion

	#region Helpers
	private static void PrintDiagnosticCounts(IDiagnosticBag diagnostics)
	{
		if (diagnostics.Count is 0)
			return;

		TextFragmentCollection fragments = [];
		fragments.Add("Example had", ClassificationKind.Message);

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
	private static string? GetExampleFile(string directory, string name)
	{
		List<string> attempts = [name];

		if (Path.HasExtension(name) is false)
			attempts.Add(Path.ChangeExtension(name, ".owl"));

		foreach (string attempt in attempts)
		{
			string file = Path.Combine(directory, attempt);
			if (File.Exists(file))
				return file;
		}

		return null;
	}
	#endregion
}
