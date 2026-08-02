using EmmyLua.LanguageServer.Framework.Protocol.Message.SelectionRange;

namespace OwlDomain.Owl.LSP.Handlers.SelectionRanges;

internal sealed partial class SelectionRangeHandler : SelectionRangeHandlerBase
{
	#region Fields
	private readonly CustomTreeHandlerBundle<SelectionRangeParams, SelectionRangeResponse> _bundle;
	#endregion

	#region Constructors
	public SelectionRangeHandler(ILspContext context)
	{
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
		serverCapabilities.SelectionRangeProvider = true;
	}
	protected override async Task<SelectionRangeResponse?> Handle(SelectionRangeParams request, CancellationToken cancellationToken)
	{
		return await _bundle.HandleAsync(request, cancellationToken);
	}
	#endregion

	#region Helpers
	[return: NotNullIfNotNull(nameof(node))]
	private static SelectionRange? GetRange(ISyntaxNode? node)
	{
		if (node is null)
			return null;

		return new()
		{
			Range = node.ToLspPosition,
			Parent = GetRange(node.Parent)
		};
	}
	#endregion
}
