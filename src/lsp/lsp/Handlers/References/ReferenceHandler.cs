using EmmyLua.LanguageServer.Framework.Protocol.Message.Reference;

namespace OwlDomain.Owl.LSP.Handlers.References;

internal sealed partial class ReferenceHandler : ReferenceHandlerBase
{
	#region Fields
	private readonly CustomTreeHandlerBundle<ReferenceParams, ReferenceResponse> _bundle;
	#endregion

	#region Constructors
	public ReferenceHandler(ILspContext context)
	{
		_bundle = new(context, request => request.TextDocument.SourcePath)
		{
			CodeHandler = new CodeHandler(),
			ConfigHandler = null
		};
	}
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.ReferencesProvider = true;
	}
	protected override async Task<ReferenceResponse?> Handle(ReferenceParams request, CancellationToken cancellationToken)
	{
		return await _bundle.HandleAsync(request, cancellationToken);
	}
	#endregion
}
