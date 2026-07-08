using System.IO;
using OwlDomain.Owl.Code.Execution.Builtins;
using OwlDomain.ParsingTools.Text.Fragments;

namespace OwlDomain.Owl.CLI.Commands.User;

[DisplayName("run")]
[Description("Runs an OWL source file")]
public sealed class RunCommand : Command<RunCommand.Settings>
{
	#region Nested types
	public sealed class Settings : CommandSettings
	{
		#region Properties
		[CommandArgument(0, "<file>")]
		public string? File { get; set; }
		#endregion
	}
	#endregion

	#region Methods
	protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
	{
		if (settings.File is null || (File.Exists(settings.File) is false))
			throw new FileNotFoundException("The OWL source file was not found.", settings.File);

		FileSystemSourceFile source = new(settings.File);

		BuiltinResolutionResult builtinResult = BuiltinResolver.Resolve();

		AnalysisContext analysis = new(builtinResult.ResultScope);
		AnalysisUpdateResult analysisResult = analysis.Update(added: [source]);

		IAnnotatedSyntaxTree? tree = analysis.Annotated.SingleOrDefault();

		IStageResult[] results = [builtinResult, analysisResult];

		if (tree is null || results.GetAllDiagnostics().Any())
		{
			Display(results.GetAllDiagnostics());
			return -1;
		}

		InterpretingResult interpreting = Interpreter.Interpret(tree);
		if (interpreting.Diagnostics.Any())
		{
			Display(interpreting.Diagnostics);
			return -1;
		}

		return 0;
	}

	private void Display(IEnumerable<IDiagnostic> diagnostics)
	{
		foreach (IDiagnostic diagnostic in diagnostics)
			Console.Error.WriteLine($"[{diagnostic.Provider.Name}/{diagnostic.Id}/{diagnostic.Kind}] ({diagnostic.Source}, {diagnostic.Position}): {diagnostic.ShortMessage.ToPlainText()}");
	}
	#endregion
}
