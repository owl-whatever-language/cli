using System.Net;
using System.Net.Sockets;

namespace OwlDomain.Owl.LSP;

public static class OwlLsp
{
	#region Functions
	public static LanguageServer Host(string version, ushort port)
	{
		Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		IPAddress ip = IPAddress.Loopback;

		IPEndPoint endpoint = new(ip, port);
		socket.Bind(endpoint);
		socket.Listen(1);

		Console.WriteLine($"Waiting for connection...");
		var client = socket.Accept();
		Console.WriteLine("Accepted connection");

		NetworkStream stream = new(client);
		LanguageServer server = LanguageServer.From(stream, stream);
		Customise(server, version);

		return server;
	}
	public static LanguageServer Stdio(string version)
	{
		LanguageServer server = LanguageServer.From(Console.OpenStandardInput(), Console.OpenStandardOutput());
		Customise(server, version);

		return server;
	}

	private static void Customise(LanguageServer server, string version)
	{
		server.OnInitialize((request, serverInfo) =>
		{
			serverInfo.Name = "OWL";
			serverInfo.Version = version;

			return Task.CompletedTask;
		});

		server.OnInitialized(async request =>
		{
			await server.Client.LogInfo("Server initialised!");
			Console.Error.WriteLine();

			Console.Error.WriteLine("Client support:");
			if (server.ClientCapabilities.TextDocument?.SemanticTokens is not null)
			{
				var semantic = server.ClientCapabilities.TextDocument.SemanticTokens;

				Console.Error.WriteLine($"- Semantic tokens ({semantic.TokenTypes.Count:n0}):");
				foreach (string token in semantic.TokenTypes)
					Console.Error.WriteLine($"  - {token}");

				Console.Error.WriteLine($"- Semantic token modifiers ({semantic.TokenModifiers.Count:n0}):");
				foreach (string modifier in semantic.TokenModifiers)
					Console.Error.WriteLine($"  - {modifier}");
			}

			Console.Error.WriteLine();

		});

		server.OnShutdown(async () =>
		{
			Console.Error.WriteLine($"Bye.");
			Environment.Exit(0);
		});

		LspContext context = new(server);

		server.AddHandler(new TextDocumentHandler(context));
		server.AddHandler(new DidChangeWatchedFilesHandler(context));
		server.AddHandler(new DocumentDiagnosticHandler(context));
		server.AddHandler(new Handlers.SemanticToken.SemanticTokensHandler(context));
		server.AddHandler(new Handlers.InlayHints.InlayHintHandler(context));
		server.AddHandler(new Handlers.Hovers.HoverHandler(context));
		server.AddHandler(new Handlers.Declarations.DeclarationHandler(context));
		server.AddHandler(new Handlers.Definitions.DefinitionHandler(context));
		server.AddHandler(new Handlers.References.ReferenceHandler(context));
		server.AddHandler(new Handlers.DocumentHighlighting.DocumentHighlightHandler(context));
		server.AddHandler(new Handlers.DocumentSymbols.DocumentSymbolHandler(context));
		server.AddHandler(new Handlers.Completions.CompletionHandler(context));
		server.AddHandler(new Handlers.Signatures.SignatureHelpHandler(context));
		server.AddHandler(new Handlers.SelectionRanges.SelectionRangeHandler(context));
		server.AddHandler(new Handlers.FoldingRanges.FoldingRangeHandler(context));
		server.AddHandler(new Handlers.CodeLenses.CodeLensHandler(context));
		server.AddHandler(new Handlers.Renames.RenameHandler(context));
	}
	#endregion
}
