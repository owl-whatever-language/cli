using EmmyLua.LanguageServer.Framework.Protocol.Message.InlayHint;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Nodes;

namespace OwlDomain.Owl.LSP.Handlers.InlayHints;

internal sealed partial class InlayHintHandler : InlayHintHandlerBase
{
	#region Constants
	private const string ReplaceWithLexeme = "replace_with_lexeme";
	#endregion

	#region Fields
	private readonly CustomTreeHandlerBundle<InlayHintParams, InlayHintResponse, InlayHint> _bundle;
	#endregion

	#region Constructors
	public InlayHintHandler(ILspContext context)
	{
		_bundle = new(context, request => request.TextDocument.SourcePath)
		{
			CodeHandler = new CodeHandler(),
			ConfigHandler = new ConfigHandler()
		};
	}
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.InlayHintProvider = new InlayHintsOptions()
		{
			ResolveProvider = true
		};
	}
	protected override async Task<InlayHint> Resolve(InlayHint request, CancellationToken cancellationToken)
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

		await _bundle.ResolveAsync(request, cancellationToken);

		return request;
	}
	protected override async Task<InlayHintResponse?> Handle(InlayHintParams request, CancellationToken cancellationToken)
	{
		return await _bundle.HandleAsync(request, cancellationToken);
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
