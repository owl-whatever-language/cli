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
		Console.Error.WriteLine($"Opened file: {request.TextDocument.Uri.Uri.AbsolutePath}");
		_context.AddFile(request.TextDocument.Uri.Uri, request.TextDocument.Text);
	}
	protected override async Task Handle(DidChangeTextDocumentParams request, CancellationToken token)
	{
		TextDocumentContentChangeEvent change = request.ContentChanges.Single();
		_context.UpdateFile(request.TextDocument.Uri.Uri, change.Text);
	}
	protected override async Task Handle(DidCloseTextDocumentParams request, CancellationToken token)
	{
		_context.RemoveFile(request.TextDocument.Uri.Uri);
	}
	protected override Task Handle(WillSaveTextDocumentParams request, CancellationToken token) => Task.CompletedTask;
	protected override Task<List<TextEdit>?> HandleRequest(WillSaveTextDocumentParams request, CancellationToken token)
	{
		List<TextEdit>? edits = null;

		return Task.FromResult(edits);
	}
	#endregion
}
