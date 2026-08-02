using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Members;

namespace OwlDomain.Owl.LSP.Handlers.SemanticToken;

partial class SemanticTokensHandler
{
	#region Nested types
	private class CodeHandler(SemanticTokensHandler handler) : BaseCustomTreeHandler<SemanticTokensParams, SemanticTokens, ICodeSyntaxTree>
	{
		#region Methods
		protected override SemanticTokens? Handle(HandlerRequest<SemanticTokensParams> request, ICodeSyntaxTree tree, CancellationToken cancellation)
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

				if (part is ISyntaxToken token)
				{
					if (token.IsDeclarationName())
					{
						modifiers.Add(SemanticTokenModifiers.Declaration);
						modifiers.Add(SemanticTokenModifiers.Definition);
					}

					if (token.Symbol is ITypeProperty)
						modifiers.Add(SemanticTokenModifiers.Readonly);
				}

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
