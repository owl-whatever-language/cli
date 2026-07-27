using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Expressions;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Statements;
using OwlDomain.ParsingTools.Positioning;
using OwlDomain.ParsingTools.Positioning.Ranges;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class FoldingRangeHandler(ILspContext context) : FoldingRangeHandlerBase
{
	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.FoldingRangeProvider = true;
	}
	protected override Task<FoldingRangeResponse> Handle(FoldingRangeParams request, CancellationToken token)
	{
		List<FoldingRange> ranges = [];
		FoldingRangeResponse response = new(ranges);

		if (_context.TryGetBundle(request.TextDocument.Uri.Uri, out ISyntaxTreeBundle? bundle) is false || bundle.LeastDetailed is null)
			return Task.FromResult(response);

		foreach (var block in bundle.LeastDetailed.Document.Flatten<IConcreteBlockStatementSyntax>())
			TryAdd(ranges, block.Start, block.End);

		foreach (var declaration in bundle.LeastDetailed.Document.Flatten<IConcreteFunctionDeclarationStatementSyntax>())
			TryAdd(ranges, declaration.Signature.Start, declaration.Signature.End);

		foreach (var call in bundle.LeastDetailed.Document.Flatten<IConcreteFunctionCallExpressionSyntax>())
			TryAdd(ranges, call.Start, call.End);

		return Task.FromResult(response);
	}
	#endregion

	#region Helpers
	private static void TryAdd(List<FoldingRange> ranges, ISyntaxToken start, ISyntaxToken end, FoldingRangeKind? kind = null)
	{
		kind ??= FoldingRangeKind.Region;

		if (start.IsFabricated || end.IsFabricated)
			return;

		PositionRange range = GetRange(start, end);
		if (range.IsMultiline is false)
			return;

		ranges.Add(GetRange(kind.Value, range));
	}
	private static PositionRange GetRange(ISyntaxToken startToken, ISyntaxToken endToken)
	{
		// Use end of start, and start of end to hopefully exclude the tokens from being folded?
		LinePosition start = startToken.Position.WithoutIndex.End;
		LinePosition end = endToken.Position.WithoutIndex.Start;

		return new(start, end);
	}
	private static FoldingRange GetRange(FoldingRangeKind kind, PositionRange position)
	{
		var start = position.Start.ToLsp;
		var end = position.End.ToLsp;

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
