using EmmyLua.LanguageServer.Framework.Protocol.Message.Reference;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class ReferenceHandler(ILspContext context) : ReferenceHandlerBase
{
	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.ReferencesProvider = true;
	}
	protected override Task<ReferenceResponse?> Handle(ReferenceParams request, CancellationToken cancellationToken)
	{
		string path = request.TextDocument.SourcePath;
		if (_context.TryGet(path, out IOwlWorkspace? workspace) is false)
			return Task.FromResult<ReferenceResponse?>(null);

		if (workspace.IsCode(path, out ICodeSyntaxTree? tree) is false)
			return Task.FromResult<ReferenceResponse?>(null);

		ISyntaxToken? target = tree.Document.Search<ISyntaxToken>(request.Position, true, token => token.Kind == SyntaxKind.Identifier);
		if (target is null)
			return Task.FromResult<ReferenceResponse?>(null);

		List<Location> locations = [];

		if (target.Symbol?.IsKnown is true)
		{
			foreach (var current in workspace.CodeContext.Annotated)
			{
				if (current.Source.TryGetUri(out Uri? uri) is false)
					continue;

				foreach (var token in current.Document.ToTokens())
				{
					if (token.Symbol == target.Symbol)
						locations.Add(new(uri, token.ToLspPosition));
				}
			}
		}

		return Task.FromResult<ReferenceResponse?>(new(locations));
	}
	#endregion
}
