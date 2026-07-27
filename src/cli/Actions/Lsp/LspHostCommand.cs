using OwlDomain.Owl.LSP;

namespace OwlDomain.Owl.CLI.Actions.Lsp;

internal sealed class LspHostCommand : Command
{
	#region Constructors
	public LspHostCommand() : base("host", "Starts an LSP server, and hosts a TCP server that LSP clients can connect to.")
	{
		Argument<ushort> portArgument = new("port")
		{
			Description = "The TCP port to host the server on.",
		};

		Add(portArgument);

		SetAction(async parsing =>
		{
			ushort port = parsing.GetRequiredValue(portArgument);
			Console.WriteLine($"Running LSP server on localhost:{port}");

			while (true)
			{
				await OwlLsp.Host(GitInfo.VersionOrMissing, port).Run();
			}
		});
	}
	#endregion
}
