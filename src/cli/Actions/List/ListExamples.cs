using System.IO;

namespace OwlDomain.Owl.CLI.Actions.List;

public class ListExamples : Command
{
	public ListExamples() : base("examples", "Lets you list the available OWL examples.")
	{
		SetAction(parsing =>
		{
			string directory = Path.Combine(AppContext.BaseDirectory, "examples");

			foreach (string file in Directory.GetFiles(directory, "*.owl"))
			{
				string name = Path.GetFileName(file);
				AnsiConsole.WriteLine(name);
			}

			AnsiConsole.WriteLine();
			AnsiConsole.MarkupLine($"[italic]⯌ When providing them to the [bold]run example[/] command, you can omit the [bold].owl[/] extension.[/]");
		});
	}
}
