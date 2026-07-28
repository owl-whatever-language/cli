using EmmyLua.LanguageServer.Framework.Protocol.Message.Definition;


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
		if (_context.TryGetTree(request.TextDocument, out ICodeSyntaxTree? tree))
		{
			ISyntaxToken? token = tree.Document.Search<ISyntaxToken>(request.Position);

			if (token?.Symbol is IDeclaredSymbol declared && declared.Declaration.TryGetLocation(out Location location))
				return Task.FromResult<DefinitionResponse?>(new(location));
		}

		return Task.FromResult<DefinitionResponse?>(null);
	}
	#endregion
}
