using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentSymbol;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions.Declared;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Loops;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Annotated;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Declared;

namespace OwlDomain.Owl.LSP.Handlers.DocumentSymbols;

partial class DocumentSymbolHandler
{
	#region Nested types
	private sealed class CodeHandler : BaseCustomTreeHandler<DocumentSymbolParams, DocumentSymbolResponse, ICodeSyntaxTree>
	{
		#region Nested types
		private sealed class Visitor : BaseAnnotatedVisitor
		{
			#region Fields
			private List<DocumentSymbol> Target { get; set; } = [];
			#endregion

			#region Functions
			public static List<DocumentSymbol> GetSymbols(IAnnotatedSyntaxTree tree)
			{
				Visitor visitor = new();
				visitor.Visit(tree);

				return visitor.Target;
			}
			#endregion

			#region Methods
			protected override bool VisitGeneral(IAnnotatedSyntaxNode node)
			{
				if (node.TryGetDeclaredSymbol(out IDeclaredSymbol? declared))
				{
					ISyntaxToken? name = declared.Declaration.Search<ISyntaxToken>(token => token.IsDeclarationName());
					if (name is null)
						ThrowHelper.ThrowInvalidOperationException($"Somehow the declaration node ({node.GetType().Name}) didn't have a declaration name, but had a declared symbol.");

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

		#region Methods
		protected override DocumentSymbolResponse? Handle(HandlerRequest<DocumentSymbolParams> request, ICodeSyntaxTree tree, CancellationToken cancellation)
		{
			List<DocumentSymbol> symbols = [];
			DocumentSymbolResponse response = new(symbols);

			if (tree is IAnnotatedSyntaxTree annotated)
			{
				List<DocumentSymbol> hierarchy = Visitor.GetSymbols(annotated);
				symbols.AddRange(hierarchy);

				return response;
			}

			foreach (IDeclaredToken token in tree.Document.Flatten<IDeclaredToken>(token => token.IsDeclarationName()))
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
				IDeclaredFunction => SymbolKind.Function,
				IDeclaredLocalVariable => SymbolKind.Variable,
				IDeclaredFunctionParameter => SymbolKind.Variable,
				IDeclaredLoopLabel => SymbolKind.Variable,

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
	#endregion
}
