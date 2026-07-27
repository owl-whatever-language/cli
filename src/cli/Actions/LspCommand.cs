using EmmyLua.LanguageServer.Framework.Server;
using OwlDomain.Owl.LSP;

namespace OwlDomain.Owl.CLI.Actions;

internal sealed class LspCommand : Command
{
	#region Constructors
	public LspCommand() : base("lsp", "Lets you start an LSP server.")
	{
		SetAction(StartServerAsync);
	}
	#endregion

	#region Methods
	private async Task StartServerAsync(ParseResult _)
	{
		LanguageServer server = OwlLsp.Create(GitInfo.Version ?? "<missing>", 8095);

		await server.Run();
	}
	#endregion
}
