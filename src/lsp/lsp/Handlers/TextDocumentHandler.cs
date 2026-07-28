namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class TextDocumentHandler(ILspContext context) : TextDocumentHandlerBase
{
	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.TextDocumentSync = new TextDocumentSyncOptions()
		{
			Change = TextDocumentSyncKind.Full,
			OpenClose = true,
		};
	}
	protected override async Task Handle(DidOpenTextDocumentParams request, CancellationToken token)
	{
		if (_context.TryGetWorkspace(request.TextDocument, out ISourceFile? source, out IOwlWorkspace? workspace))
		{
			// Note(Nightowl):
			// The current file was opened by the IDE, we need to switch
			// from manually managing it, to letting the IDE manage it;
			workspace.RemoveFile(source);
		}
		else
			workspace = _context.NewWorkspace();

		WorkspaceSourceFile file = new(request.TextDocument.Uri.Uri, request.TextDocument.Text);
		workspace.AddFile(file);

		workspace.Analyse();
	}
	protected override async Task Handle(DidChangeTextDocumentParams request, CancellationToken token)
	{
		TextDocumentContentChangeEvent change = request.ContentChanges.Single();

		if (_context.TryGetWorkspace(request.TextDocument, out ISourceFile? file, out IOwlWorkspace? workspace))
		{
			WorkspaceSourceFile workspaceFile = (WorkspaceSourceFile)file;
			workspaceFile.Text = change.Text;

			workspace.UpdateFile(file);
		}
		else
		{
			workspace = _context.NewWorkspace();

			file = new WorkspaceSourceFile(request.TextDocument.Uri.Uri, change.Text);
			workspace.AddFile(file);
		}

		workspace.Analyse();
	}
	protected override async Task Handle(DidCloseTextDocumentParams request, CancellationToken token)
	{
		if (_context.TryGetWorkspace(request.TextDocument, out ISourceFile? file, out IOwlWorkspace? workspace))
		{
			workspace.RemoveFile(file);
			workspace.Analyse();
		}
	}
	protected override Task Handle(WillSaveTextDocumentParams request, CancellationToken token) => Task.CompletedTask;
	protected override Task<List<TextEdit>?> HandleRequest(WillSaveTextDocumentParams request, CancellationToken token)
	{
		List<TextEdit>? edits = null;

		return Task.FromResult(edits);
	}
	#endregion
}
