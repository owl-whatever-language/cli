using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentHighlight;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Expressions;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.FunctionArguments;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Statements;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class DocumentHighlightHandler(ILspContext context) : DocumentHighlightHandlerBase
{
	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.DocumentHighlightProvider = true;
	}
	protected override Task<DocumentHighlightResponse> Handle(DocumentHighlightParams request, CancellationToken cancellation)
	{
		List<DocumentHighlight> highlights = [];
		DocumentHighlightResponse response = new(highlights);

		if (_context.TryGetBundle(request.TextDocument.Uri.Uri, out ISyntaxTreeBundle? bundle) is false || bundle.LeastDetailed is null)
			return Task.FromResult(response);

		ISyntaxToken? target = bundle.LeastDetailed.Document.Search<ISyntaxToken>(token => token.Position.WithoutIndex.Contains(request.Position.ToOwl));
		if (target is null || target.Symbol?.IsKnown is not true)
			return Task.FromResult(response);

		foreach (ISyntaxToken token in bundle.LeastDetailed.Document.ToTokens().Where(t => t.Symbol == target.Symbol))
		{
			DocumentHighlightKind kind = token.Parent switch
			{
				IConcreteGetExpressionSyntax => DocumentHighlightKind.Read,
				IConcreteAssignmentExpressionSyntax => DocumentHighlightKind.Write,
				IConcreteVariableDeclarationStatementSyntax => DocumentHighlightKind.Write,
				IConcreteNamedFunctionArgumentSyntax => DocumentHighlightKind.Write,

				_ => DocumentHighlightKind.Text
			};

			highlights.Add(new()
			{
				Kind = kind,
				Range = token.Position.ToLsp,
			});
		}

		return Task.FromResult(response);
	}
	#endregion
}
