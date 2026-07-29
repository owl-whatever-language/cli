using EmmyLua.LanguageServer.Framework.Protocol.Message.Rename;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class RenameHandler(ILspContext context) : RenameHandlerBase
{
	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.RenameProvider = new RenameOptions()
		{
			PrepareProvider = true
		};
	}
	protected override Task<PrepareRenameResponse> Handle(PrepareRenameParams request, CancellationToken token)
	{
		if (_context.TryGetTree(request.TextDocument, out ICodeSyntaxTree? tree) is false)
			return Task.FromResult(new PrepareRenameResponse(false));

		ISyntaxToken? target = tree.Document.Search<ISyntaxToken>(request.Position, true, token => token.Kind == SyntaxKind.Identifier);
		if (target is not null && target.Symbol is IDeclaredSymbol && target.Value is string text)
			return Task.FromResult(new PrepareRenameResponse(target.ToLspPosition, text));

		return Task.FromResult(new PrepareRenameResponse(false));
	}
	protected override Task<WorkspaceEdit?> Handle(RenameParams request, CancellationToken cancellation)
	{
		if (_context.TryGetTree(request.TextDocument, out ICodeSyntaxTree? tree, out IOwlWorkspace? workspace) is false)
			return Task.FromResult<WorkspaceEdit?>(null);

		ISyntaxToken? target = tree.Document.Search<ISyntaxToken>(request.Position, true, token => token.Kind == SyntaxKind.Identifier);
		if (target is null || target.Symbol is not IDeclaredSymbol symbol)
			return Task.FromResult<WorkspaceEdit?>(null);

		WorkspaceEdit result = new() { Changes = [] };

		foreach (var current in workspace.CodeContext.Annotated)
		{
			if (current.Source.TryGetUri(out DocumentUri uri) is false)
				continue;

			foreach (var token in current.Document.ToTokens())
			{
				if (token.Symbol != target.Symbol)
					continue;


				if (result.Changes.TryGetValue(uri, out List<TextEdit>? edits) is false)
				{
					edits = [];
					result.Changes.Add(uri, edits);
				}

				edits.Add(new()
				{
					Range = token.ToLspPosition,
					NewText = request.NewName
				});
			}
		}

		return Task.FromResult<WorkspaceEdit?>(result);
	}
	#endregion
}
