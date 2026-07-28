using EmmyLua.LanguageServer.Framework.Protocol.Message.Definition;
using EmmyLua.LanguageServer.Framework.Protocol.Model;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class DefinitionHandler(ILspContext context) : DefinitionHandlerBase
{
	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.DefinitionProvider = true;
	}
	protected override Task<DefinitionResponse?> Handle(DefinitionParams request, CancellationToken cancellationToken)
	{
		if (_context.TryGetBundle(request.TextDocument.Uri.Uri, out ISyntaxTreeBundle? bundle) is false || bundle.LeastDetailed is null)
			return Task.FromResult<DefinitionResponse?>(null);

		ISyntaxToken? token = bundle.LeastDetailed.Document.Search<ISyntaxToken>(request.Position);
		if (token is null)
			return Task.FromResult<DefinitionResponse?>(null);

		if (token.Symbol is IDeclaredSymbol declared)
		{
			if (_context.TryGetUri(declared.Declaration.GetTree().Source, out Uri? uri))
				return Task.FromResult<DefinitionResponse?>(new(new Location(uri, declared.Declaration.ToLspPosition)));
		}

		return Task.FromResult<DefinitionResponse?>(null);
	}
	#endregion
}
