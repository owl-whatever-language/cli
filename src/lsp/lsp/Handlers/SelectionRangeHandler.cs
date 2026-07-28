using EmmyLua.LanguageServer.Framework.Protocol.Message.SelectionRange;
using EmmyLua.LanguageServer.Framework.Protocol.Model;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class SelectionRangeHandler(ILspContext context) : SelectionRangeHandlerBase
{
	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.SelectionRangeProvider = true;
	}
	protected override Task<SelectionRangeResponse?> Handle(SelectionRangeParams request, CancellationToken cancellationToken)
	{
		if (_context.TryGetBundle(request.TextDocument.Uri.Uri, out ISyntaxTreeBundle? bundle) is false || bundle.LeastDetailed is null)
			return Task.FromResult<SelectionRangeResponse?>(null);

		List<SelectionRange> ranges = [];
		SelectionRangeResponse response = new(ranges);

		foreach (Position target in request.Positions)
		{
			ISyntaxPart? part = bundle.LeastDetailed.Document.Search<ISyntaxPart>(target);
			if (part is not null)
				ranges.Add(GetRange(part));
			else
				ranges.Add(new());
		}

		return Task.FromResult<SelectionRangeResponse?>(response);
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
