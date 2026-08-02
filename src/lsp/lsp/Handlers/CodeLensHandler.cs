using EmmyLua.LanguageServer.Framework.Protocol.Message.CodeLens;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class CodeLensHandler(ILspContext context) : CodeLensHandlerBase
{
	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.CodeLensProvider = new()
		{
			ResolveProvider = true,
		};
	}
	protected override Task<CodeLens> Resolve(CodeLens request, CancellationToken token)
	{
		return Task.FromResult(request);
	}
	protected override Task<CodeLensResponse> Handle(CodeLensParams request, CancellationToken token)
	{
		List<CodeLens> lenses = [];
		CodeLensResponse response = new(lenses);

		if (_context.TryGet(request.TextDocument.SourcePath, out IOwlWorkspace? workspace))
		{
			// Note(Nightowl): To fill later;
		}

		return Task.FromResult(response);
	}
	#endregion
}
