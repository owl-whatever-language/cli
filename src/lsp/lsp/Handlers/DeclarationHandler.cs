using EmmyLua.LanguageServer.Framework.Protocol.Message.Declaration;
using EmmyLua.LanguageServer.Framework.Protocol.Model;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class DeclarationHandler(ILspContext context) : DeclarationHandlerBase
{
	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.DeclarationProvider = true;
	}
	protected override Task<DeclarationResponse?> Handle(DeclarationParams request, CancellationToken cancellationToken)
	{
		if (_context.TryGetBundle(request.TextDocument.Uri.Uri, out ISyntaxTreeBundle? bundle) is false || bundle.LeastDetailed is null)
			return Task.FromResult<DeclarationResponse?>(null);

		ISyntaxToken? token = bundle.LeastDetailed.Document.Search<ISyntaxToken>(token => token.Position.WithoutIndex.Contains(request.Position.ToOwl));
		if (token is null)
			return Task.FromResult<DeclarationResponse?>(null);

		if (token.Symbol is IDeclaredSymbol declared)
		{
			if (_context.TryGetUri(declared.Declaration.GetTree().Source, out Uri? uri))
				return Task.FromResult<DeclarationResponse?>(new(new Location(uri, declared.Declaration.Position.ToLsp)));
		}

		return Task.FromResult<DeclarationResponse?>(null);
	}
	#endregion
}
