using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentSymbol;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Declared.Statements;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class DocumentSymbolHandler(ILspContext context) : DocumentSymbolHandlerBase
{
	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.DocumentSymbolProvider = true;
	}
	protected override Task<DocumentSymbolResponse> Handle(DocumentSymbolParams request, CancellationToken token)
	{
		List<DocumentSymbol> symbols = [];
		DocumentSymbolResponse response = new(symbols);

		if (_context.TryGetBundle(request.TextDocument.Uri.Uri, out ISyntaxTreeBundle? bundle) is false || bundle.LeastDetailed is null)
			return Task.FromResult(response);

		foreach (var function in bundle.LeastDetailed.Document.Flatten<IDeclaredFunctionDeclarationStatementSyntax>())
		{
			if (string.IsNullOrWhiteSpace(function.Function.Name))
				continue;

			symbols.Add(new()
			{
				Kind = SymbolKind.Function,
				Name = function.Function.Name,
				Range = function.ToLspPosition,
				SelectionRange = function.Signature.Name.ToLspPosition
			});
		}

		foreach (var variable in bundle.LeastDetailed.Document.Flatten<IDeclaredVariableDeclarationStatementSyntax>())
		{
			if (string.IsNullOrWhiteSpace(variable.Variable.Name))
				continue;

			symbols.Add(new()
			{
				Kind = SymbolKind.Variable,
				Name = variable.Variable.Name,
				Range = variable.ToLspPosition,
				SelectionRange = variable.Name.ToLspPosition
			});
		}

		return Task.FromResult(response);
	}
	#endregion
}
