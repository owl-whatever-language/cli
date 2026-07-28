using EmmyLua.LanguageServer.Framework.Protocol.Message.Hover;
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
		if (_context.TryGetTree(request.TextDocument, out ICodeSyntaxTree? tree) is false)
			return Task.FromResult<HoverResponse?>(null);

		ISyntaxToken? token = tree.Document.Search<ISyntaxToken>(request.Position);
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

		if (kind is not null)
			parts.Add($"`({kind})`");

		if (token.Symbol is not null)
			parts.Add($"```owl\n{token.Symbol.GetDebugText().ToPlainText()}\n```");

		if (token.Symbol is IDeclaredSymbol declared)
		{
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
