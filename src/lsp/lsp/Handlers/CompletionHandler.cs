using CommunityToolkit.Diagnostics;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using OwlDomain.Owl.Code.CodeAnalysis.Parsing;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Callable;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Members;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Semantic.Expressions;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Semantic.FunctionArguments;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Semantic.Nodes;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Semantic.Statements;
using OwlDomain.ParsingTools.Positioning;

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
			TriggerCharacters = [".", "(", ","],
		};
	}
	protected override Task<CompletionResponse?> Handle(CompletionParams request, CancellationToken cancellation)
	{
		if (_context.TryGetBundle(request.TextDocument.Uri.Uri, out ISyntaxTreeBundle? bundle) is false || bundle.LeastDetailed is null)
			return Task.FromResult<CompletionResponse?>(null);

		bool IsTargetToken(ISyntaxToken token)
		{
			LinePosition targetPosition = request.Position.ToOwl;

			if (token.Position.WithoutIndex.Start == targetPosition)
				return true;

			if (token.Position.WithoutIndex.End == targetPosition)
				return true;

			if (token.Position.WithoutIndex.Contains(targetPosition))
				return true;

			return false;
		}

		ISyntaxNode? target = bundle.LeastDetailed.Document.Search<ISyntaxToken>(IsTargetToken);

		if (target is ISyntaxToken token && token.Kind == SyntaxKind.StringText)
			return Task.FromResult<CompletionResponse?>(null);

		if (target is not null)
		{
			if (target.Parent is ISemanticMemberAccessExpressionSyntax access)
			{
				if (access.Expression is ISemanticGetExpressionSyntax get && get.ResultType.IsNotError)
				{
					List<CompletionItem> members = [];
					FromTypeAccess(members, get, access);
					return Task.FromResult<CompletionResponse?>(new(members));
				}
			}
		}

		List<CompletionItem> completions = [];

		if (target?.Parent is ISemanticFunctionCallExpressionSyntax or ISemanticFunctionArgumentSyntax)
		{
			var call = target.GetParent<ISemanticFunctionCallExpressionSyntax>();
			if (call?.Callable is not null)
			{
				AddParameterNames(completions, call.Callable);
			}
		}

		ISyntaxNode? contextNode = target ?? bundle.LeastDetailed.Document.Search(node => node.Position.WithoutIndex.Contains(request.Position.ToOwl));
		ISymbolScope? scope = contextNode.GetChain().Select(TrySelectScope).FirstOrDefault(scope => scope is not null);

		AddKeywords(completions);

		if (scope is not null)
			FromScope(completions, scope);

		return Task.FromResult<CompletionResponse?>(new(completions));
	}
	protected override Task<CompletionItem> Resolve(CompletionItem item, CancellationToken token)
	{
		return Task.FromResult(item);
	}
	#endregion

	#region Helpers
	private void AddParameterNames(List<CompletionItem> items, ICallableType callable)
	{
		foreach (ICallableTypeParameter parameter in callable.Parameters)
		{
			if (string.IsNullOrWhiteSpace(parameter.Name))
				continue;

			items.Add(new()
			{
				Kind = CompletionItemKind.Reference,
				Label = parameter.Name + ":"
			});
		}
	}
	private void AddKeywords(List<CompletionItem> items)
	{
		foreach (SyntaxKind keyword in SyntaxKind.AllKeywords)
		{
			items.Add(new()
			{
				Kind = CompletionItemKind.Keyword,
				Label = keyword.Name
			});
		}
	}
	private ISymbolScope? TrySelectScope(ISyntaxNode node)
	{
		return node switch
		{
			ISemanticFunctionDeclarationStatementSyntax function => function.Scope,
			ISemanticDocumentSyntax document => document.Scope,

			_ => null,
		};
	}
	private void FromScope(List<CompletionItem> completions, ISymbolScope scope)
	{
		foreach (IGrouping<string?, ISymbol> group in scope.GetNamed().All.GroupBy(s => s.Name))
		{
			if (string.IsNullOrWhiteSpace(group.Key))
				continue;

			CompletionItem completion = GetCompletion(group.First());
			completions.Add(completion);
		}
	}
	private void FromTypeAccess(List<CompletionItem> completions, ISemanticGetExpressionSyntax get, ISemanticMemberAccessExpressionSyntax access)
	{
		foreach (ITypeMember member in get.ResultType.Members)
		{
			if (string.IsNullOrWhiteSpace(member.Name))
				continue;

			CompletionItem completion = GetCompletion(member);
			completions.Add(completion);
		}
	}
	private CompletionItem GetCompletion(ISymbol symbol)
	{
		Debug.Assert(symbol.Name is not null);

		CompletionItemKind kind = symbol switch
		{
			ITypeMethod => CompletionItemKind.Method,
			ITypeProperty => CompletionItemKind.Property,
			ILocalVariable => CompletionItemKind.Variable,
			IFunctionParameter => CompletionItemKind.Variable,
			IFunction => CompletionItemKind.Function,
			IType => CompletionItemKind.Class,

			_ => ThrowHelper.ThrowArgumentException<CompletionItemKind>($"Unhandled symbol type ({symbol.GetType().Name}).")
		};

		return new()
		{
			Kind = kind,
			Label = symbol.Name,
			Detail = symbol.GetDebugText().ToPlainText()
		};
	}
	#endregion
}
