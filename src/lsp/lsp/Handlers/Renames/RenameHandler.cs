using EmmyLua.LanguageServer.Framework.Protocol.Message.Rename;

namespace OwlDomain.Owl.LSP.Handlers.Renames;

internal sealed partial class RenameHandler : RenameHandlerBase
{
	#region Fields
	private readonly CustomTreeHandlerBundle<PrepareRenameParams, PrepareRenameResponse> _prepareBundle;
	private readonly CustomTreeHandlerBundle<RenameParams, WorkspaceEdit> _bundle;
	#endregion

	#region Constructors
	public RenameHandler(ILspContext context)
	{
		_prepareBundle = new(context, request => request.TextDocument.SourcePath)
		{
			CodeHandler = new PrepareCodeHandler(),
			ConfigHandler = new PrepareConfigHandler(),
		};

		_bundle = new(context, request => request.TextDocument.SourcePath)
		{
			CodeHandler = new CodeHandler(),
			ConfigHandler = new ConfigHandler(),
		};
	}
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.RenameProvider = new RenameOptions()
		{
			PrepareProvider = true
		};
	}
	protected override async Task<PrepareRenameResponse> Handle(PrepareRenameParams request, CancellationToken token)
	{
		return await _prepareBundle.HandleAsync(request, token) ?? new(false);
	}
	protected override async Task<WorkspaceEdit?> Handle(RenameParams request, CancellationToken cancellation)
	{
		return await _bundle.HandleAsync(request, cancellation);
	}
	#endregion

	#region Helpers
	private static IEnumerable<ISyntaxTree> GetAllTrees(IOwlWorkspace workspace)
	{
		foreach (ISyntaxTree tree in workspace.CodeContext.Trees)
			yield return tree;

		foreach (ISyntaxTree tree in workspace.WorkspaceContext.Trees)
			yield return tree;

		foreach (ISyntaxTree tree in workspace.ConfigContext.Trees)
			yield return tree;

		foreach (ISyntaxTree tree in workspace.PackageContext.Trees)
			yield return tree;
	}
	private static WorkspaceEdit? GetEdit(IOwlWorkspace workspace, ISymbol target, string newName)
	{
		WorkspaceEdit result = new() { Changes = [] };

		foreach (var current in GetAllTrees(workspace))
		{
			if (current.Source.TryGetUri(out DocumentUri uri) is false)
				continue;

			foreach (var token in current.Document.ToTokens())
			{
				if (token.Symbol != target)
					continue;

				if (result.Changes.TryGetValue(uri, out List<TextEdit>? edits) is false)
				{
					edits = [];
					result.Changes.Add(uri, edits);
				}

				edits.Add(new()
				{
					Range = token.ToLspPosition,
					NewText = newName
				});
			}
		}

		if (result.Changes.Any())
			return result;

		return null;
	}
	#endregion
}
