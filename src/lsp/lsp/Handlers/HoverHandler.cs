using EmmyLua.LanguageServer.Framework.Protocol.Message.Hover;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Markup;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Members;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class HoverHandler(ILspContext context) : HoverHandlerBase
{
	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.HoverProvider = true;
	}
	protected override Task<HoverResponse?> Handle(HoverParams request, CancellationToken cancellation)
	{
		if (_context.TryGetBundle(request.TextDocument.Uri.Uri, out ISyntaxTreeBundle? bundle) is false || bundle.LeastDetailed is null)
			return Task.FromResult<HoverResponse?>(null);

		ISyntaxToken? token = bundle.LeastDetailed.Document.Search<ISyntaxToken>(token => token.Position.WithoutIndex.Contains(request.Position.ToOwl));
		if (token is null)
			return Task.FromResult<HoverResponse?>(null);

		List<string> parts = [];

		string? kind = token.Symbol switch
		{
			ILocalVariable => "variable",
			IFunction => "function",
			IFunctionParameter => "parameter",
			ITypeProperty => "property",
			ITypeMethod => "method",
			IType => "type",

			_ => null,
		};

		if (token.Symbol is IDeclaredSymbol declared)
		{
			parts.Add($"```owl\n{token.Symbol.GetDebugText().ToPlainText()}\n```");

			string comments = string.Join("\n",
				declared.Declaration
				.ToTokens()
				.FirstOrDefault()
				?.LeadingTrivia
				.Where(t => t.Kind == SyntaxKind.Comment)
				.Select(t => t.Value as string)
				.Where(v => v is not null)!);

			if (comments.Length > 0)
				parts.Add(comments);
		}
		else if (kind is not null)
			parts.Add($"`({kind})`");

		if (parts.Any())
		{
			return Task.FromResult<HoverResponse?>(new()
			{
				Contents = new()
				{
					Kind = MarkupKind.Markdown,
					Value = string.Join("\n\n", parts)
				}
			});
		}

		return Task.FromResult<HoverResponse?>(null);
	}
	#endregion
}
