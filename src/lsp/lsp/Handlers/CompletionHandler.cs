using CommunityToolkit.Diagnostics;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using OwlDomain.Owl.Code.CodeAnalysis.Parsing;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Callable;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Members;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Semantic.Expressions;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Semantic.FunctionArguments;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Loops;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Declared.Nodes;

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
			TriggerCharacters = [".", "(", ",", ":"],
		};
	}
	protected override Task<CompletionResponse?> Handle(CompletionParams request, CancellationToken cancellation)
	{
		if (_context.TryGetTree(request.TextDocument, out ICodeSyntaxTree? tree) is false)
			return Task.FromResult<CompletionResponse?>(null);

		ISyntaxNode? target = tree.Document.Search<ISyntaxToken>(request.Position);

		if (target is ISyntaxToken token && token.Kind == SyntaxKind.StringText)
			return Task.FromResult<CompletionResponse?>(null);

		if (target is not null)
		{
			if (target.Parent is ISemanticMemberAccessExpressionSyntax access && (target == access.Dot || target == access.Name))
			{
				List<CompletionItem> members = [];

				if (access.Expression.ResultType.IsNotError)
					FromTypeAccess(members, access.Expression.ResultType, access);

				return Task.FromResult<CompletionResponse?>(new(members));
			}
		}

		List<CompletionItem> completions = [];

		if (target?.Parent is ISemanticFunctionCallExpressionSyntax or ISemanticFunctionArgumentSyntax)
		{
			var call = target.GetParent<ISemanticFunctionCallExpressionSyntax>();
			if (call?.Callable is not null)
				AddParameterNames(completions, call.Callable);
			else if (call?.Expression is ISemanticGetExpressionSyntax get)
			{
				foreach (ISymbol candidate in get.Candidates)
				{
					if (candidate is ICallableType callable)
						AddParameterNames(completions, callable);
					else if (candidate is IFunction function)
						AddParameterNames(completions, function.AsCallable);
				}
			}
		}

		ISyntaxNode? contextNode = target ?? tree.Document.Search(request.Position);
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
	private ISymbolScope? TrySelectScope(ISyntaxNode node)
	{
		if (node is IDeclaredDocumentSyntax document)
			return document.Scope;

		return node.TryGetDeclaredScope();
	}
	private void AddParameterNames(List<CompletionItem> items, ICallableType callable)
	{
		foreach (ICallableTypeParameter parameter in callable.Parameters)
		{
			if (string.IsNullOrWhiteSpace(parameter.Name))
				continue;

			string label = $"{parameter.Name}:";

			if (items.Any(i => i.Label == label))
				continue;

			items.Add(new()
			{
				Kind = CompletionItemKind.Reference,
				Label = label,
				Detail = parameter.GetDebugText().ToPlainText()
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
	private void FromTypeAccess(List<CompletionItem> completions, IType type, ISemanticMemberAccessExpressionSyntax access)
	{
		foreach (ITypeMember member in type.Members)
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
			ILoopLabel => CompletionItemKind.Reference,

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
