using EmmyLua.LanguageServer.Framework.Protocol.Message.Definition;

namespace OwlDomain.Owl.LSP.Handlers.Definitions;

internal sealed partial class DefinitionHandler : DefinitionHandlerBase
{
	#region Fields
	private readonly CustomTreeHandlerBundle<DefinitionParams, DefinitionResponse> _bundle;
	#endregion

	#region Constructors
	public DefinitionHandler(ILspContext context)
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
	protected override async Task<DefinitionResponse?> Handle(DefinitionParams request, CancellationToken cancellationToken)
	{
		return await _bundle.HandleAsync(request, cancellationToken);
	}
	#endregion
}
