using EmmyLua.LanguageServer.Framework.Protocol.Message.InlayHint;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Annotated.FunctionArguments;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class InlayHintHandler(ILspContext context) : InlayHintHandlerBase
{
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
	protected override Task<InlayHint> Resolve(InlayHint request, CancellationToken cancellationToken) => Task.FromResult(request);
	protected override Task<InlayHintResponse?> Handle(InlayHintParams request, CancellationToken cancellationToken)
	{
		List<InlayHint> hints = [];

		if (_context.TryGetBundle(request.TextDocument.Uri.Uri, out ISyntaxTreeBundle? bundle) is false || bundle.LeastDetailed is null)
			return Task.FromResult<InlayHintResponse?>(new(hints));

		foreach (var argument in bundle.LeastDetailed.Document.Flatten<IAnnotatedRegularFunctionArgumentSyntax>())
		{
			string? name = argument.Parameter?.Name;
			if (name is null)
				continue;

			InlayHint hint = new()
			{
				Kind = InlayHintKind.Parameter,
				Position = argument.Position.Start.ToLsp,
				PaddingRight = true,
				Label = $"name:"
			};

			hints.Add(hint);
		}

		return Task.FromResult<InlayHintResponse?>(new(hints));
	}
	#endregion
}
