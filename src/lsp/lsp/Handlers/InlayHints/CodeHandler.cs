using EmmyLua.LanguageServer.Framework.Protocol.Message.InlayHint;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Annotated.FunctionArguments;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Nodes;

namespace OwlDomain.Owl.LSP.Handlers.InlayHints;

partial class InlayHintHandler
{
	#region Nested types
	private sealed class CodeHandler : BaseCustomTreeHandler<InlayHintParams, InlayHintResponse, ICodeSyntaxTree, InlayHint>
	{
		#region Methods
		protected override InlayHintResponse? Handle(HandlerRequest<InlayHintParams> request, ICodeSyntaxTree tree, CancellationToken cancellation)
		{
			List<InlayHint> hints = [];

			// Note(Nightowl):
			// This is a cool idea, but disable it for now since it makes writing the code very confusing.
			// Perhaps it could be added back in later on, when adding a debounce on the text edits to
			// make this only show up when not actively typing...
			// AddMissingTokens(hints, tree);

			foreach (var argument in tree.Document.Flatten<IAnnotatedRegularFunctionArgumentSyntax>())
			{
				string? name = argument.Parameter?.Name;
				if (name is null)
					continue;

				InlayHint hint = new()
				{
					Kind = InlayHintKind.Parameter,
					Position = argument.ToLspPosition.Start,
					PaddingRight = true,
					Label = $"{name}:",
					Data = ReplaceWithLexeme
				};

				hints.Add(hint);
			}

			foreach (var signature in tree.Document.Flatten<IConcreteFunctionDeclarationSignatureSyntax>(signature => signature.Return is null))
			{
				InlayHint hint = new()
				{
					Kind = InlayHintKind.Type,
					Position = signature.End.ToLspPosition.End,
					Label = ": void",
					// Data = ReplaceWithLexeme // Note(Nightowl): Don't allow replacing this yet since typing void as a type isn't actually supported yet;
				};

				hints.Add(hint);
			}

			foreach (var label in tree.Document.Flatten<IConcreteLoopLabelClauseSyntax>(label => label.IsFabricated))
			{
				InlayHint hint = new()
				{
					Position = label.ToLspPosition.Start,
					Label = ": loop",
					Data = ReplaceWithLexeme
				};

				hints.Add(hint);
			}

			return new(hints);
		}
		#endregion
	}
	#endregion
}
