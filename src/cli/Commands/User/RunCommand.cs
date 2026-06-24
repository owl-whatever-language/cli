using System.IO;

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

		CompilationContext compilation = new();
		CompilationUpdateResult compilationResult = compilation.Update(added: [source]);

		IFinalSyntaxTree? tree = compilation.Trees.Values.Single().Final;
		if (tree is null || compilationResult.GetAllDiagnostics().Any())
		{
			Display(compilationResult.GetAllDiagnostics());
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
