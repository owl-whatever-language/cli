using EmmyLua.LanguageServer.Framework.Protocol.Message.InlayHint;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Annotated.FunctionArguments;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Nodes;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class InlayHintHandler(ILspContext context) : InlayHintHandlerBase
{
	#region Constants
	private const string ReplaceWithLexeme = "replace_with_lexeme";
	#endregion

	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.InlayHintProvider = new InlayHintsOptions()
		{
			ResolveProvider = true
		};
	}
	protected override Task<InlayHint> Resolve(InlayHint request, CancellationToken cancellationToken)
	{
		if (request.Data?.Value as string == ReplaceWithLexeme && request.Label.String is string label)
		{
			if (request.PaddingLeft is true)
				label = " " + label;

			if (request.PaddingRight is true)
				label += " ";

			request.TextEdits ??= [];
			request.TextEdits.Add(new()
			{
				Range = new(request.Position, request.Position),
				NewText = label
			});
		}

		return Task.FromResult(request);
	}
	protected override Task<InlayHintResponse?> Handle(InlayHintParams request, CancellationToken cancellationToken)
	{
		string path = request.TextDocument.SourcePath;
		if (_context.TryGet(path, out IOwlWorkspace? workspace) is false)
			return Task.FromResult<InlayHintResponse?>(null);

		if (workspace.IsCode(path, out ICodeSyntaxTree? tree) is false)
			return Task.FromResult<InlayHintResponse?>(null);

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

		return Task.FromResult<InlayHintResponse?>(new(hints));
	}
	#endregion

	#region Helpers
	private static void AddMissingTokens(List<InlayHint> hints, ICodeSyntaxTree tree)
	{
		foreach (var token in tree.Document.Flatten<ISyntaxToken>(token => token.IsFabricated))
		{
			if (token.Parent is IConcreteLoopLabelClauseSyntax)
				continue;

			string? lexeme = GetMissingLexeme(token.Kind);

			if (lexeme is null)
				continue;

			InlayHint hint = new()
			{
				Position = token.ToLspPosition.Start,
				Label = lexeme,
				Data = ReplaceWithLexeme
			};

			hints.Add(hint);
		}
	}
	private static string? GetMissingLexeme(SyntaxKind kind)
	{
		if (kind == SyntaxKind.Semicolon)
			return ";";

		if (kind == SyntaxKind.EqualSign)
			return "=";

		if (kind == SyntaxKind.OpenBracket)
			return "(";

		if (kind == SyntaxKind.CloseBracket)
			return ")";

		if (kind == SyntaxKind.OpenBrace)
			return "{";

		if (kind == SyntaxKind.CloseBrace)
			return "}";

		return null;
	}
	#endregion
}
