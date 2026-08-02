using EmmyLua.LanguageServer.Framework.Protocol.Message.SignatureHelp;

namespace OwlDomain.Owl.LSP.Handlers.Signatures;

internal sealed partial class SignatureHelpHandler : SignatureHelpHandlerBase
{
	#region Fields
	private readonly CustomTreeHandlerBundle<SignatureHelpParams, SignatureHelp> _bundle;
	#endregion

	#region Constructors
	public SignatureHelpHandler(ILspContext context)
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
		serverCapabilities.SignatureHelpProvider = new()
		{
			TriggerCharacters = ["("],
			RetriggerCharacters = ["(", ","],
		};
	}
	protected override async Task<SignatureHelp> Handle(SignatureHelpParams request, CancellationToken token)
	{
		return await _bundle.HandleAsync(request, token) ?? new();
	}
	#endregion
}
