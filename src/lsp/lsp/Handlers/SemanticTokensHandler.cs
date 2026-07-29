using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;
using OwlDomain.Owl.Code.CodeAnalysis.Annotation.Flags;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Members;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class SemanticTokensHandler(ILspContext context) : SemanticTokensHandlerBase
{
	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Properties
	private static Dictionary<ClassificationKind, string> Classifications { get; } = new()
	{
		{ ClassificationKind.Comment, SemanticTokenTypes.Comment },
		{ ClassificationKind.Keyword, SemanticTokenTypes.Keyword },

		{ ClassificationKind.Operator, SemanticTokenTypes.Operator },
		{ ClassificationKind.String, SemanticTokenTypes.String },
		{ ClassificationKind.Number, SemanticTokenTypes.Number },
		{ ClassificationKind.Boolean, SemanticTokenTypes.Keyword },

		{ ClassificationKind.Parameter, SemanticTokenTypes.Parameter },
		{ ClassificationKind.TypeProperty, SemanticTokenTypes.Property },
		{ ClassificationKind.Function, SemanticTokenTypes.Function },
		{ ClassificationKind.Variable, SemanticTokenTypes.Variable },
		{ ClassificationKind.TypeMethod, SemanticTokenTypes.Method },
		{ ClassificationKind.Label, SemanticTokenTypes.Variable },

		{ ClassificationKind.Type, SemanticTokenTypes.Type},
	};
	private static List<string> TokenTypes { get; } = Classifications.Values.ToList();
	private static List<string> TokenModifiers { get; } =
	[
		SemanticTokenModifiers.Declaration, SemanticTokenModifiers.Definition,
		SemanticTokenModifiers.Readonly,
	];
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.SemanticTokensProvider = new()
		{
			Legend = new()
			{
				TokenTypes = TokenTypes,
				TokenModifiers = TokenModifiers
			},

			Full = true,
		};
	}
	protected override Task<SemanticTokens?> Handle(SemanticTokensParams semanticTokensParams, CancellationToken cancellationToken)
	{
		if (_context.TryGetTree(semanticTokensParams.TextDocument, out ICodeSyntaxTree? tree) is false)
			return Task.FromResult<SemanticTokens?>(null);

		SemanticTokensBuilder builder = new(TokenTypes, TokenModifiers);

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

		return Task.FromResult<SemanticTokens?>(new()
		{
			Data = builder.Build()
		});
	}
	protected override Task<SemanticTokensDeltaResponse?> Handle(SemanticTokensDeltaParams semanticTokensDeltaParams, CancellationToken cancellationToken) => throw new NotImplementedException();
	protected override Task<SemanticTokens?> Handle(SemanticTokensRangeParams semanticTokensRangeParams, CancellationToken cancellationToken) => throw new NotImplementedException();
	#endregion

	#region Helpers
	private static void Convert(ClassificationKind classification, out string? type, out string? modifier)
	{
		modifier = null;

		foreach (ClassificationKind current in classification.Iterate())
		{
			if (Classifications.TryGetValue(current, out type))
				return;
		}

		type = null;
	}
	#endregion
}
