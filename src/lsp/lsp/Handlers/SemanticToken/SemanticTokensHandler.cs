using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;
using OwlDomain.Owl.Config.CodeAnalysis.Classification;

namespace OwlDomain.Owl.LSP.Handlers.SemanticToken;

internal sealed partial class SemanticTokensHandler : SemanticTokensHandlerBase
{
	#region Fields
	private readonly CustomTreeHandlerBundle<SemanticTokensParams, SemanticTokens> _handlers;
	#endregion

	#region Constructors
	public SemanticTokensHandler(ILspContext context)
	{
		_handlers = new(context, request => request.TextDocument.SourcePath)
		{
			CodeHandler = new CodeHandler(this),
			ConfigHandler = new ConfigHandler(this)
		};
	}
	#endregion

	#region Properties
	private static Dictionary<ClassificationKind, string> Classifications { get; } = new()
	{
		// Note(Nightowl): For code;
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

// Note(Nightowl): For config;
		{ ClassificationKind.Key, SemanticTokenTypes.Property },
		{ ClassificationKind.Value, SemanticTokenTypes.String },
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
	protected override async Task<SemanticTokens?> Handle(SemanticTokensParams semanticTokensParams, CancellationToken cancellationToken)
	{
		return await _handlers.HandleAsync(semanticTokensParams, cancellationToken);
	}
	protected override Task<SemanticTokensDeltaResponse?> Handle(SemanticTokensDeltaParams semanticTokensDeltaParams, CancellationToken cancellationToken) => throw new NotImplementedException();
	protected override Task<SemanticTokens?> Handle(SemanticTokensRangeParams semanticTokensRangeParams, CancellationToken cancellationToken) => throw new NotImplementedException();

	#endregion

	#region Helpers
	private SemanticTokensBuilder GetBuilder() => new(TokenTypes, TokenModifiers);
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
