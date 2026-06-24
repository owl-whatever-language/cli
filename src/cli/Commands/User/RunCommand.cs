using System.IO;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Builtins;

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

		CompilationContext compilation = new(builtinResult.ResultScope);
		CompilationUpdateResult compilationResult = compilation.Update(added: [source]);

		ISyntaxTree? tree = compilation.Trees.Values.Single().MostDetailed;

		IStageResult[] results = [builtinResult, compilationResult];

		if (tree is null || results.GetAllDiagnostics().Any())
		{
			Display(results.GetAllDiagnostics());
			return -1;
		}

		throw new NotImplementedException("Interpreting has not been added back in yet.");
	}

	private void Display(IEnumerable<IDiagnostic> diagnostics)
	{
		foreach (IDiagnostic diagnostic in diagnostics)
			Console.Error.WriteLine($"[{diagnostic.Provider.Name}/{diagnostic.Id}/{diagnostic.Kind}] ({diagnostic.Location}): {diagnostic.Message}");
	}
	#endregion
}
