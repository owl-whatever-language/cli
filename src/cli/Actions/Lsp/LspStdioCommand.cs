using OwlDomain.Owl.LSP;

namespace OwlDomain.Owl.CLI.Actions.Lsp;

internal sealed class LspStdioCommand : Command
{
	#region Constructors
	public LspStdioCommand() : base("stdio", "Starts an LSP server that uses standard IO for the communication.")
	{
		SetAction(async parsing =>
		{
			if (Console.IsOutputRedirected is false)
			{
				Console.Error.WriteLine("The standard output was not redirected, so you likely ran this command incorrectly.");
				return -1;
			}

			if (Console.IsInputRedirected is false)
			{
				Console.Error.WriteLine("The standard input was not redirected, so you likely ran this command incorrectly.");
				return -1;
			}

			await OwlLsp.Stdio(GitInfo.VersionOrMissing).Run();
			return 0;
		});
	}
	#endregion
}
