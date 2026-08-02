using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace OwlDomain.Owl.LSP.Handlers.SemanticToken;

partial class SemanticTokensHandler
{
	#region Nested types
	private class ConfigHandler(SemanticTokensHandler handler) : BaseCustomTreeHandler<SemanticTokensParams, SemanticTokens, IConfigSyntaxTree>
	{
		#region Methods
		protected override SemanticTokens? Handle(HandlerRequest<SemanticTokensParams> request, IConfigSyntaxTree tree, CancellationToken cancellation)
		{
			SemanticTokensBuilder builder = handler.GetBuilder();

			foreach (ISyntaxPart part in tree.Document.ToParts())
			{
				if (part.Classification is null)
					continue;

				Convert(part.Classification.Value, out string? type, out string? modifier);

				if (type is null)
					continue;

				HashSet<string> modifiers = modifier is null ? [] : [modifier];

				builder.Push(part.ToLspPosition.Start, part.Position.Length, type, modifiers.ToList());
			}

			return new()
			{
				Data = builder.Build()
			};
		}
		#endregion
	}
	#endregion
}
