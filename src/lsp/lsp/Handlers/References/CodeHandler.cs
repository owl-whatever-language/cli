using EmmyLua.LanguageServer.Framework.Protocol.Message.Reference;

namespace OwlDomain.Owl.LSP.Handlers.References;

partial class ReferenceHandler
{
	#region Nested types
	private sealed class CodeHandler : BaseCustomTreeHandler<ReferenceParams, ReferenceResponse, ICodeSyntaxTree>
	{
		#region Methods
		protected override ReferenceResponse? Handle(HandlerRequest<ReferenceParams> request, ICodeSyntaxTree tree, CancellationToken cancellation)
		{
			ISyntaxToken? target = tree.Document.Search<ISyntaxToken>(request.Request.Position, true, token => token.Kind == SyntaxKind.Identifier);
			if (target is null)
				return null;

			List<Location> locations = [];

			if (target.Symbol?.IsKnown is true)
			{
				foreach (var current in request.Workspace.CodeContext.Annotated)
				{
					if (current.Source.TryGetUri(out Uri? uri) is false)
						continue;

					foreach (var token in current.Document.ToTokens())
					{
						if (token.Symbol == target.Symbol)
							locations.Add(new(uri, token.ToLspPosition));
					}
				}
			}

			return new(locations);
		}
		#endregion
	}
	#endregion
}
