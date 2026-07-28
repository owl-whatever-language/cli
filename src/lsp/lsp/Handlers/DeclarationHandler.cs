using EmmyLua.LanguageServer.Framework.Protocol.Message.Declaration;

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
		if (_context.TryGetTree(request.TextDocument, out ICodeSyntaxTree? tree))
		{
			ISyntaxToken? token = tree.Document.Search<ISyntaxToken>(request.Position);

			if (token?.Symbol is IDeclaredSymbol declared && declared.Declaration.TryGetLocation(out Location location))
				return Task.FromResult<DeclarationResponse?>(new(location));
		}

		return Task.FromResult<DeclarationResponse?>(null);
	}
	#endregion
}
