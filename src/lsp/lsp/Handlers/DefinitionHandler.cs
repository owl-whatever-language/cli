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
		DefinitionResponse? response = null;

		string path = request.TextDocument.SourcePath;
		if (_context.TryGet(path, out IOwlWorkspace? workspace))
		{
			if (workspace.IsCode(path, out ICodeSyntaxTree? code))
				response = ForCode(request, code);
		}

		return Task.FromResult(response);
	}
	#endregion

	#region Code methods
	private DefinitionResponse? ForCode(DefinitionParams request, ICodeSyntaxTree tree)
	{
		ISyntaxToken? token = tree.Document.Search<ISyntaxToken>(request.Position, true, token => token.Kind == SyntaxKind.Identifier);

		if (token?.Symbol is IDeclaredSymbol declared && declared.Declaration.TryGetLocation(out Location location))
			return new(location);

		return null;
	}
	#endregion
}
