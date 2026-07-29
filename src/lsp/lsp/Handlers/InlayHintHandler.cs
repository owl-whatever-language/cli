using EmmyLua.LanguageServer.Framework.Protocol.Message.InlayHint;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Annotated.FunctionArguments;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Nodes;

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

		if (_context.TryGetTree(request.TextDocument, out ICodeSyntaxTree? tree) is false)
			return Task.FromResult<InlayHintResponse?>(new(hints));

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
			};

			hints.Add(hint);
		}

		foreach (var signature in tree.Document.Flatten<IConcreteFunctionDeclarationSignatureSyntax>(signature => signature.Return is null))
		{
			InlayHint hint = new()
			{
				Kind = InlayHintKind.Type,
				Position = signature.End.ToLspPosition.End,
				PaddingLeft = true,
				Label = ": void",
			};

			hints.Add(hint);
		}

		foreach (var label in tree.Document.Flatten<IConcreteLoopLabelClauseSyntax>(label => label.IsFabricated))
		{
			InlayHint hint = new()
			{
				Position = label.ToLspPosition.Start,
				PaddingLeft = true,
				Label = ": loop",
			};

			hints.Add(hint);
		}


		return Task.FromResult<InlayHintResponse?>(new(hints));
	}
	#endregion
}
