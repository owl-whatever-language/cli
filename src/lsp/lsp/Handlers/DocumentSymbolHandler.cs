using CommunityToolkit.Diagnostics;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentSymbol;

namespace OwlDomain.Owl.LSP.Handlers;

using CodeDeclared = Code.CodeAnalysis.Syntax.Declared;
using CodeAnnotated = Code.CodeAnalysis.Syntax.Annotated;
using CodeSemantics = Code.CodeAnalysis.Semantics;

internal sealed class DocumentSymbolHandler(ILspContext context) : DocumentSymbolHandlerBase
{
	#region Nested types
	private sealed class Visitor : CodeAnnotated.BaseAnnotatedVisitor
	{
		#region Fields
		private List<DocumentSymbol> Target { get; set; } = [];
		#endregion

		#region Functions
		public static List<DocumentSymbol> GetSymbols(CodeAnnotated.IAnnotatedSyntaxTree tree)
		{
			Visitor visitor = new();
			visitor.Visit(tree);

			return visitor.Target;
		}
		#endregion

		#region Methods
		protected override bool VisitGeneral(CodeAnnotated.IAnnotatedSyntaxNode node)
		{
			if (node.TryGetDeclaredSymbol(out IDeclaredSymbol? declared))
			{
				ISyntaxToken? name = declared.Declaration.Search<ISyntaxToken>(token => token.IsDeclarationName());
				if (name is null)
					ThrowHelper.ThrowInvalidOperationException($"Somehow the declaration type () didn't have a declaration name, but had a declared symbol.");

				DocumentSymbol symbol = CreateSymbol(declared, name);
				Target.Add(symbol);

				if (node.TryGetDeclaredScope(out _))
				{
					var old = Target;
					Target = symbol.Children ??= [];

					VisitChildren(node);

					Target = old;

					return false;
				}
			}

			return true;
		}
		#endregion
	}
	#endregion

	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.DocumentSymbolProvider = true;
	}
	protected override Task<DocumentSymbolResponse> Handle(DocumentSymbolParams request, CancellationToken cancellation)
	{
		DocumentSymbolResponse? response = null;

		string path = request.TextDocument.SourcePath;
		if (_context.TryGet(path, out IOwlWorkspace? workspace))
		{
			if (workspace.IsCode(path, out ICodeSyntaxTree? code))
				response = ForCode(code);
		}

		response ??= new([]);
		return Task.FromResult(response);
	}
	#endregion

	#region Code methods
	private DocumentSymbolResponse ForCode(ICodeSyntaxTree tree)
	{
		List<DocumentSymbol> symbols = [];
		DocumentSymbolResponse response = new(symbols);

		if (tree is CodeAnnotated.IAnnotatedSyntaxTree annotated)
		{
			List<DocumentSymbol> hierarchy = Visitor.GetSymbols(annotated);
			symbols.AddRange(hierarchy);

			return response;
		}

		foreach (CodeDeclared.IDeclaredToken token in tree.Document.Flatten<CodeDeclared.IDeclaredToken>(token => token.IsDeclarationName()))
		{
			if (string.IsNullOrWhiteSpace(token.Symbol?.Name) || token.Symbol is not IDeclaredSymbol declared)
				continue;

			DocumentSymbol symbol = CreateSymbol(declared, token);
			symbols.Add(symbol);
		}

		return response;
	}
	private static DocumentSymbol CreateSymbol(IDeclaredSymbol symbol, ISyntaxToken node)
	{
		Debug.Assert(symbol.Name is not null);

		SymbolKind kind = symbol switch
		{
			CodeSemantics.Functions.Declared.IDeclaredFunction => SymbolKind.Function,
			CodeSemantics.Functions.Declared.IDeclaredLocalVariable => SymbolKind.Variable,
			CodeSemantics.Functions.Declared.IDeclaredFunctionParameter => SymbolKind.Variable,
			CodeSemantics.Loops.IDeclaredLoopLabel => SymbolKind.Variable,

			_ => ThrowHelper.ThrowInvalidOperationException<SymbolKind>($"Unhandled symbol type ({symbol.GetType().Name}).")
		};

		return new()
		{
			Kind = kind,
			Name = symbol.Name,
			Range = symbol.Declaration.ToLspPosition,
			SelectionRange = node.ToLspPosition
		};
	}
	#endregion
}
