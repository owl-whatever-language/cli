using EmmyLua.LanguageServer.Framework.Protocol.Message.Definition;

namespace OwlDomain.Owl.LSP.Handlers.Definitions;

partial class DefinitionHandler
{
	#region Nested types
	private sealed class CodeHandler : BaseCustomTreeHandler<DefinitionParams, DefinitionResponse, ICodeSyntaxTree>
	{
		#region Methods
		protected override DefinitionResponse? Handle(HandlerRequest<DefinitionParams> request, ICodeSyntaxTree tree, CancellationToken cancellation)
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
