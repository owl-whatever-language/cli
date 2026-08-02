using EmmyLua.LanguageServer.Framework.Protocol.Message.Declaration;

namespace OwlDomain.Owl.LSP.Handlers.Declarations;

partial class DeclarationHandler
{
	#region Nested types
	private sealed class CodeHandler : BaseCustomTreeHandler<DeclarationParams, DeclarationResponse, ICodeSyntaxTree>
	{
		#region Methods
		protected override DeclarationResponse? Handle(HandlerRequest<DeclarationParams> request, ICodeSyntaxTree tree, CancellationToken cancellation)
		{
			ISyntaxToken? token = tree.Document.Search<ISyntaxToken>(request.Request.Position, true, token => token.Kind == SyntaxKind.Identifier);

			if (token?.Symbol is IDeclaredSymbol declared && declared.Declaration.TryGetLocation(out Location location))
				return new(location);

			return null;
		}
		#endregion
	}
	#endregion
}
