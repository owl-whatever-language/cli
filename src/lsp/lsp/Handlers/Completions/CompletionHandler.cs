using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;

namespace OwlDomain.Owl.LSP.Handlers.Completions;

internal sealed partial class CompletionHandler : CompletionHandlerBase
{
	#region Fields
	private readonly CustomTreeHandlerBundle<CompletionParams, CompletionResponse, CompletionItem> _bundle;
	#endregion

	#region Constructors
	public CompletionHandler(ILspContext context)
	{
		_bundle = new(context, request => request.TextDocument.SourcePath)
		{
			CodeHandler = new CodeHandler(),
			ConfigHandler = null
		};
	}
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		const string alphabet = "abcdefghijklmnopqrstuvwxyz";

		HashSet<string> characters =
		[
			".", "(", ",", ":", "{", "}", ")", "+", "/", "-", "*", "%", "<", ">", "=", "!", " ",
			"$", "_",

			..alphabet.Select(c => c.ToString()),
			..alphabet.ToUpper().Select(c => c.ToString()),
		];

		serverCapabilities.CompletionProvider = new()
		{
			TriggerCharacters = characters.ToList(),
		};
	}
	protected override async Task<CompletionItem> Resolve(CompletionItem item, CancellationToken token)
	{
		return await _bundle.ResolveAsync(item, token) ?? item;
	}
	protected override async Task<CompletionResponse?> Handle(CompletionParams request, CancellationToken cancellation)
	{
		return await _bundle.HandleAsync(request, cancellation);
	}
	#endregion
}
