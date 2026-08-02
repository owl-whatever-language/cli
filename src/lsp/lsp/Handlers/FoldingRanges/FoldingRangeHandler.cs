using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;

namespace OwlDomain.Owl.LSP.Handlers.FoldingRanges;

internal sealed partial class FoldingRangeHandler : FoldingRangeHandlerBase
{
	#region Fields
	private readonly CustomTreeHandlerBundle<FoldingRangeParams, FoldingRangeResponse> _bundle;
	#endregion

	#region Constructors
	public FoldingRangeHandler(ILspContext context)
	{
		_bundle = new(context, request => request.TextDocument.SourcePath)
		{
			CodeHandler = new CodeHandler(),
			ConfigHandler = null
		};
	}
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.FoldingRangeProvider = true;
	}
	protected override async Task<FoldingRangeResponse> Handle(FoldingRangeParams request, CancellationToken cancellationToken)
	{
		FoldingRangeResponse? response = await _bundle.HandleAsync(request, cancellationToken);
		return response ?? new([]);
	}
	#endregion

	#region Helpers
	private static void TryAdd(List<FoldingRange> ranges, ISyntaxToken start, ISyntaxToken end, FoldingRangeKind? kind = null)
	{
		kind ??= FoldingRangeKind.Region;

		if (start.IsFabricated || end.IsFabricated)
			return;

		DocumentRange range = GetRange(start, end);
		if (range.Start.Line == range.End.Line)
			return;

		ranges.Add(GetRange(kind.Value, range));
	}
	private static DocumentRange GetRange(ISyntaxToken startToken, ISyntaxToken endToken)
	{
		// Use end of start, and start of end to hopefully exclude the tokens from being folded?
		Position start = startToken.ToLspPosition.End;
		Position end = endToken.ToLspPosition.Start;

		return new(start, end);
	}
	private static FoldingRange GetRange(FoldingRangeKind kind, DocumentRange position)
	{
		var start = position.Start;
		var end = position.End;

		return new()
		{

			Kind = kind,

			StartLine = (uint)start.Line,
			StartCharacter = (uint)start.Character,

			EndLine = (uint)end.Line,
			EndCharacter = (uint)end.Character,
		};
	}
	#endregion
}
