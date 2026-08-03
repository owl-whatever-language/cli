using EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceFolders;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class WorkspaceFolderHandler : WorkspaceFolderHandlerBase
{
	#region Fields
	private readonly ILspContext _context;
	#endregion

	#region Constructors
	public WorkspaceFolderHandler(ILspContext context)
	{
		_context = context;
	}
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.Workspace ??= new();
		serverCapabilities.Workspace.WorkspaceFolders = new()
		{
			Supported = true,
			ChangeNotifications = Guid.NewGuid().ToString()
		};
	}
	protected override Task Handle(DidChangeWorkspaceFoldersParams request, CancellationToken token)
	{
		foreach (WorkspaceFolder folder in request.Event.Removed)
			_context.StopWatchingWorkspace(folder.Uri.SourcePath);

		foreach (WorkspaceFolder folder in request.Event.Added)
			_context.WatchWorkspace(folder.Uri.SourcePath);

		return Task.CompletedTask;
	}
	#endregion
}
