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

		if (_context.TryGetTree(request.TextDocument, out ICodeSyntaxTree? tree) is false)
			return Task.FromResult(response);

		ISyntaxToken? target = tree.Document.Search<ISyntaxToken>(request.Position, true, token => token.Kind == SyntaxKind.Identifier);
		if (target is null)
			return Task.FromResult(response);

		if (target.Parent is IConcreteReturnStatementSyntax @return && @return.Keyword == target)
		{
			var function = target.GetChain().OfType<IConcreteFunctionDeclarationStatementSyntax>().FirstOrDefault();
			if (function is not null)
			{
				highlights.Add(new() { Kind = DocumentHighlightKind.Text, Range = target.ToLspPosition });
				highlights.Add(new() { Kind = DocumentHighlightKind.Text, Range = function.Signature.Name.ToLspPosition });
			}

			return Task.FromResult(response);
		}

		if (target.Symbol?.IsKnown is not true)
			return Task.FromResult(response);

		foreach (ISyntaxToken token in tree.Document.ToTokens().Where(t => t.Symbol == target.Symbol))
		{
			DocumentHighlightKind kind = token.Parent switch
			{
				IConcreteGetExpressionSyntax get => get.Parent is IConcreteAssignmentExpressionSyntax or IConcreteCompoundAssignmentExpressionSyntax ? DocumentHighlightKind.Write : DocumentHighlightKind.Read,
				IConcreteAssignmentExpressionSyntax => DocumentHighlightKind.Write,
				IConcreteVariableDeclarationStatementSyntax => DocumentHighlightKind.Write,
				IConcreteNamedFunctionArgumentSyntax => DocumentHighlightKind.Write,
				IConcreteMemberAccessExpressionSyntax access => access.Parent is IConcreteAssignmentExpressionSyntax or IConcreteCompoundAssignmentExpressionSyntax ? DocumentHighlightKind.Write : DocumentHighlightKind.Read,

				_ => DocumentHighlightKind.Text
			};

			highlights.Add(new()
			{
				Kind = kind,
				Range = token.ToLspPosition,
			});
		}

		return Task.FromResult(response);
	}
	#endregion
}
