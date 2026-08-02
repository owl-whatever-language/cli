using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Expressions;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Statements;

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
		FoldingRangeResponse? response = null;

		string path = request.TextDocument.SourcePath;
		if (_context.TryGet(path, out IOwlWorkspace? workspace))
		{
			if (workspace.IsCode(path, out ICodeSyntaxTree? code))
				response = ForCode(code);
		}

		response ??= new([]);
		return Task.FromResult(response);
	}
	private FoldingRangeResponse ForCode(ICodeSyntaxTree tree)
	{
		List<FoldingRange> ranges = [];
		FoldingRangeResponse response = new(ranges);

		foreach (var block in tree.Document.Flatten<IConcreteBlockStatementSyntax>())
			TryAdd(ranges, block.Start, block.End);

		foreach (var declaration in tree.Document.Flatten<IConcreteFunctionDeclarationStatementSyntax>())
			TryAdd(ranges, declaration.Signature.Start, declaration.Signature.End);

		foreach (var call in tree.Document.Flatten<IConcreteFunctionCallExpressionSyntax>())
			TryAdd(ranges, call.Start, call.End);

		return response;
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
