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
	protected override Task Handle(DidOpenTextDocumentParams request, CancellationToken token)
	{
		Console.Error.WriteLine($"Client wants to manage: {request.TextDocument.SourcePath}");
		_context.AddFile(request.TextDocument.SourcePath, request.TextDocument.Text);
		return Task.CompletedTask;
	}
	protected override async Task Handle(DidChangeTextDocumentParams request, CancellationToken token)
	{
		TextDocumentContentChangeEvent change = request.ContentChanges.Single();
		_context.UpdateFile(request.TextDocument.SourcePath, change.Text, request.TextDocument.Version);
	}
	protected override async Task Handle(DidCloseTextDocumentParams request, CancellationToken token)
	{
		Console.Error.WriteLine($"Client wants to stop managing: {request.TextDocument.SourcePath}");
		_context.RemoveFile(request.TextDocument.SourcePath);
	}
	protected override Task Handle(WillSaveTextDocumentParams request, CancellationToken token) => Task.CompletedTask;
	protected override Task<List<TextEdit>?> HandleRequest(WillSaveTextDocumentParams request, CancellationToken token)
	{
		List<TextEdit>? edits = null;

		return Task.FromResult(edits);
	}
	#endregion
}
