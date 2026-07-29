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
		});

		server.OnShutdown(async () =>
		{
			Console.Error.WriteLine($"Bye.");
			Environment.Exit(0);
		});

		LspContext context = new(server);

		server.AddHandler(new TextDocumentHandler(context));
		server.AddHandler(new SemanticTokensHandler(context));
		server.AddHandler(new InlayHintHandler(context));
		server.AddHandler(new HoverHandler(context));
		server.AddHandler(new DocumentDiagnosticHandler(context));
		server.AddHandler(new DeclarationHandler(context));
		server.AddHandler(new DefinitionHandler(context));
		server.AddHandler(new ReferenceHandler(context));
		server.AddHandler(new DocumentHighlightHandler(context));
		server.AddHandler(new DocumentSymbolHandler(context));
		server.AddHandler(new CompletionHandler(context));
		server.AddHandler(new SignatureHelpHandler(context));
		server.AddHandler(new SelectionRangeHandler(context));
		server.AddHandler(new FoldingRangeHandler(context));
		server.AddHandler(new CodeLensHandler(context));
	}
	#endregion
}
