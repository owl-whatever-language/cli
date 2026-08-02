using EmmyLua.LanguageServer.Framework.Protocol.Message.CodeLens;

namespace OwlDomain.Owl.LSP.Handlers.CodeLenses;

internal sealed partial class CodeLensHandler : CodeLensHandlerBase
{
	#region Fields
	private readonly CustomTreeHandlerBundle<CodeLensParams, CodeLensResponse, CodeLens> _bundle;
	#endregion

	#region Constructors
	public CodeLensHandler(ILspContext context)
	{
		_bundle = new(context, request => request.TextDocument.SourcePath)
		{
			CodeHandler = null,
			ConfigHandler = null,
		};
	}
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.CodeLensProvider = new()
		{
			ResolveProvider = true,
		};
	}
	protected override async Task<CodeLens> Resolve(CodeLens request, CancellationToken token)
	{
		return await _bundle.ResolveAsync(request, token) ?? request;
	}
	protected override async Task<CodeLensResponse> Handle(CodeLensParams request, CancellationToken token)
	{
		return await _bundle.HandleAsync(request, token) ?? new([]);
	}
	#endregion
}
