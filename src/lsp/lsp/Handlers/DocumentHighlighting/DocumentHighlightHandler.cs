using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentHighlight;

namespace OwlDomain.Owl.LSP.Handlers.DocumentHighlighting;

internal sealed partial class DocumentHighlightHandler : DocumentHighlightHandlerBase
{
	#region Fields
	private readonly CustomTreeHandlerBundle<DocumentHighlightParams, DocumentHighlightResponse> _bundle;
	#endregion

	#region Constructors
	public DocumentHighlightHandler(ILspContext context)
	{
		_bundle = new(context, request => request.TextDocument.SourcePath)
		{
			CodeHandler = null,
			ConfigHandler = null
		};
	}
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.DocumentHighlightProvider = true;
	}
	protected override async Task<DocumentHighlightResponse> Handle(DocumentHighlightParams request, CancellationToken cancellation)
	{
		DocumentHighlightResponse? response = await _bundle.HandleAsync(request, cancellation);

		return response ?? new([]);
	}
	#endregion
}
