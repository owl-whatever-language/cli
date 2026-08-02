using EmmyLua.LanguageServer.Framework.Protocol.Message.Hover;

namespace OwlDomain.Owl.LSP.Handlers.Hovers;

internal sealed partial class HoverHandler : HoverHandlerBase
{
	#region Fields
	private readonly CustomTreeHandlerBundle<HoverParams, HoverResponse> _bundle;
	#endregion

	#region Constructors
	public HoverHandler(ILspContext context)
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
		serverCapabilities.HoverProvider = true;
	}
	protected override async Task<HoverResponse?> Handle(HoverParams request, CancellationToken cancellation)
	{
		return await _bundle.HandleAsync(request, cancellation) ?? GetBasicResponse();
	}
	private static HoverResponse GetBasicResponse()
	{
		return new()
		{
			Contents = new()
			{
				Kind = MarkupKind.Markdown,
				Value = "*Nothing interesting to show.*"
			}
		};
	}
	#endregion
}
