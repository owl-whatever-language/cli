using EmmyLua.LanguageServer.Framework.Protocol.Message.SelectionRange;
using OwlDomain.ParsingTools.Positioning;

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

		foreach (LinePosition target in request.Positions.Select(p => p.ToOwl))
		{
			ISyntaxPart? part = bundle.LeastDetailed.Document.Search<ISyntaxPart>(part => part.Position.WithoutIndex.Contains(target));
			if (part is not null)
			{
				ranges.Add(GetRange(part));
			}
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
			Range = node.Position.ToLsp,
			Parent = GetRange(node.Parent)
		};
	}
	#endregion
}
