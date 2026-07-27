using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Members;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Semantic.Expressions;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class CompletionHandler(ILspContext context) : CompletionHandlerBase
{
	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.CompletionProvider = new()
		{
			TriggerCharacters = ["."],
		};
	}
	protected override Task<CompletionResponse?> Handle(CompletionParams request, CancellationToken cancellation)
	{
		if (_context.TryGetBundle(request.TextDocument.Uri.Uri, out ISyntaxTreeBundle? bundle) is false || bundle.LeastDetailed is null)
			return Task.FromResult<CompletionResponse?>(null);

		ISyntaxToken? target = bundle.LeastDetailed.Document.Search<ISyntaxToken>(token => token.Position.WithoutIndex.Contains(request.Position.ToOwl));
		if (target is null)
			return Task.FromResult<CompletionResponse?>(null);

		if (target.Kind == SyntaxKind.Period && target.Parent is ISemanticMemberAccessExpressionSyntax access)
		{
			if (access.Expression is ISemanticGetExpressionSyntax get && get.ResultType.IsNotError)
			{
				List<CompletionItem> completions = [];

				foreach (ITypeMember member in get.ResultType.Members)
				{
					if (string.IsNullOrWhiteSpace(member.Name))
						continue;

					completions.Add(new()
					{
						Kind = member is ITypeMethod ? CompletionItemKind.Method : CompletionItemKind.Property,
						Label = member.Name,
						Detail = member.GetDebugText().ToPlainText()
					});
				}

				return Task.FromResult<CompletionResponse?>(new(completions));
			}
		}

		return Task.FromResult<CompletionResponse?>(null);
	}
	protected override Task<CompletionItem> Resolve(CompletionItem item, CancellationToken token)
	{
		return Task.FromResult(item);
	}
	#endregion
}
