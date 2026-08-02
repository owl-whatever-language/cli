using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentSymbol;

namespace OwlDomain.Owl.LSP.Handlers.DocumentSymbols;

internal sealed partial class DocumentSymbolHandler : DocumentSymbolHandlerBase
{
	#region fields
	private readonly CustomTreeHandlerBundle<DocumentSymbolParams, DocumentSymbolResponse> _bundle;
	#endregion

	#region Constructors
	public DocumentSymbolHandler(ILspContext context)
	{
		_bundle = new(context, request => request.TextDocument.SourcePath)
		{
			CodeHandler = new CodeHandler(),
			ConfigHandler = null,
		};
	}
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.DocumentSymbolProvider = true;
	}
	protected override async Task<DocumentSymbolResponse> Handle(DocumentSymbolParams request, CancellationToken cancellation)
	{
		DocumentSymbolResponse? response = await _bundle.HandleAsync(request, cancellation);
		return response ?? new([]);
	}
	#endregion
}
