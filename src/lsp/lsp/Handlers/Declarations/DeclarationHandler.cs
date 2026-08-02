using EmmyLua.LanguageServer.Framework.Protocol.Message.Declaration;

namespace OwlDomain.Owl.LSP.Handlers.Declarations;

internal sealed partial class DeclarationHandler : DeclarationHandlerBase
{
	#region Fields
	private readonly CustomTreeHandlerBundle<DeclarationParams, DeclarationResponse> _bundle;
	#endregion

	#region Constructors
	public DeclarationHandler(ILspContext context)
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
		serverCapabilities.DeclarationProvider = true;
	}
	protected override async Task<DeclarationResponse?> Handle(DeclarationParams request, CancellationToken cancellationToken)
	{
		return await _bundle.HandleAsync(request, cancellationToken);
	}
	#endregion
}
