using EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceWatchedFile;
using EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceWatchedFile.Watch;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class DidChangeWatchedFilesHandler(ILspContext context) : DidChangeWatchedFilesHandlerBase
{
	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities) { }
	public override void RegisterDynamicCapability(LanguageServer server, ClientCapabilities clientCapabilities)
	{
		DidChangeWatchedFilesRegistrationOptions registration = new()
		{
			Watchers =
			[
				new FileSystemWatcher()
				{
					GlobalPattern = "**/owl.{workspace,package,config}",
					Kind = WatchKind.Create | WatchKind.Change | WatchKind.Delete
				},
				new FileSystemWatcher()
				{
					GlobalPattern = "**/*.owl",
					Kind = WatchKind.Create | WatchKind.Change | WatchKind.Delete
				},
			]
		};

		server.Client.DynamicRegisterCapability(new()
		{
			Registrations =
			[
				new()
				{
					Id = Guid.NewGuid().ToString(),
					Method = "workspace/didChangeWatchedFiles",
					RegisterOptions = registration
				}
			]
		});
	}
	protected override Task Handle(DidChangeWatchedFilesParams request, CancellationToken token)
	{
		foreach (FileEvent change in request.Changes)
		{
			string path = change.Uri.SourcePath;

			if (change.Type is FileChangeType.Created)
			{
				string text = System.IO.File.ReadAllText(path);
				_context.AddFile(path, text);
			}
			else if (change.Type is FileChangeType.Changed)
			{
				string text = System.IO.File.ReadAllText(path);
				_context.UpdateFile(path, text, null);
			}
			else if (change.Type is FileChangeType.Deleted)
				_context.RemoveFile(path);
		}

		return Task.CompletedTask;
	}
	#endregion
}
