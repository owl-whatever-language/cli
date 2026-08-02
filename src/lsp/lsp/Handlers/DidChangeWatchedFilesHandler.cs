using EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceWatchedFile;
using EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceWatchedFile.Watch;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class DidChangeWatchedFilesHandler : DidChangeWatchedFilesHandlerBase
{
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
					GlobalPattern = "owl.workspace",
					Kind = WatchKind.Create | WatchKind.Change | WatchKind.Delete
				},
				new FileSystemWatcher()
				{
					GlobalPattern = "owl.package",
					Kind = WatchKind.Create | WatchKind.Change | WatchKind.Delete
				},
				new FileSystemWatcher()
				{
					GlobalPattern = "owl.config",
					Kind = WatchKind.Create | WatchKind.Change | WatchKind.Delete
				},
				new FileSystemWatcher()
				{
					GlobalPattern = "*.owl",
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
		return Task.CompletedTask;
	}
	#endregion
}
