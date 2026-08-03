using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;
using OwlDomain.Owl.Code.CodeAnalysis.Parsing;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions.Callable;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Expressions;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Statements;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Semantic.Expressions;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Semantic.Statements;
using OwlDomain.ParsingTools.Positioning;

namespace OwlDomain.Owl.LSP.Handlers.Completions;

partial class CompletionHandler
{
	#region Nested types
	private sealed class CodeHandler : BaseCustomTreeHandler<CompletionParams, CompletionResponse, ICodeSyntaxTree, CompletionItem>
	{
		#region Methods
		protected override CompletionResponse? Handle(HandlerRequest<CompletionParams> request, ICodeSyntaxTree tree, CancellationToken cancellation)
		{
			ISyntaxToken? target = SelectTarget(tree, request.Request.Position);
			if (target is null || ShouldIgnore(target))
				return null;

			Console.Error.WriteLine($"Completion target: {target.Kind.Name}");

			List<CompletionItem> items = [];
			CompletionList list = new() { Items = items };

			TryAddCompletions(items, target);

			return items.Any() ? new(list) : null;
		}
		private void TryAddCompletions(List<CompletionItem> items, ISyntaxToken target)
		{
			TryAddKeyword(items, target);
			TryAddForFunctionCall(items, target);
		}
		#endregion

		#region Function methods
		private void TryAddForFunctionCall(List<CompletionItem> items, ISyntaxToken target)
		{
			if (IsArgument(target, out IConcreteFunctionCallExpressionSyntax? call))
			{
				List<IFunction> functions = [];
				if (call is ISemanticFunctionCallExpressionSyntax semantic)
				{
					if (semantic.Callable is ICallableFunction callable)
						functions.Add(callable.Function);
					else if (semantic.Expression is ISemanticGetExpressionSyntax get)
						functions.AddRange(get.Candidates.OfType<IFunction>());
				}

				IEnumerable<IFunctionParameter> parameters = functions
					.SelectMany(f => f.Parameters)
					.Where(p => string.IsNullOrWhiteSpace(p.Name) is false)
					.DistinctBy(p => p.Name);

				foreach (IGrouping<string?, IFunctionParameter> group in functions.SelectMany(f => f.Parameters).GroupBy(p => p.Name))
				{
					if (string.IsNullOrWhiteSpace(group.Key))
						continue;

					IType? type = group.Select(p => p.Type).Distinct().SingleOrDefault();

					CompletionItem item = new()
					{
						Kind = CompletionItemKind.Reference,
						Label = group.Key,
						InsertText = $"{group.Key}: ",
						Detail = type?.GetDebugText().ToPlainText()
					};

					int count = group.Count();
					if (count > 1)
						item.LabelDetails = new() { Detail = $"{count} definitions" };

					items.Add(item);
				}
			}
		}
		private bool IsArgument(ISyntaxToken target, [NotNullWhen(true)] out IConcreteFunctionCallExpressionSyntax? call)
		{
			call = target.GetChain().OfType<IConcreteFunctionCallExpressionSyntax>().FirstOrDefault();
			if (call is null)
				return false;

			if (target == call.Start || call.Arguments.Separators.Contains(target))
				return true;

			return false;
		}
		#endregion

		#region Keyword methods
		private static bool ShouldAddElse(ISyntaxToken target)
		{
			if ((target.Kind == SyntaxKind.CloseBrace || target.Kind == SyntaxKind.Semicolon) && target.Parent?.Parent is IConcreteIfStatementSyntax or IConcreteIfElseStatementSyntax)
				return true;

			return false;
		}
		private void TryAddKeyword(List<CompletionItem> items, ISyntaxToken target)
		{
			if (IsStatement(target) is false)
				return;

			items.Add(new()
			{
				Kind = CompletionItemKind.Keyword,
				Label = "if",
				Detail = "if (...)",
				InsertText = "if ($1) $0",
				InsertTextFormat = InsertTextFormat.Snippet,
				Documentation = new MarkupContent()
				{
					Kind = MarkupKind.Markdown,
					Value = "The `if` and `else` keywords are used for controlling *if* code should run, based on a condition that you give it."
				}
			});

			if (ShouldAddElse(target))
			{
				items.Add(new()
				{
					Kind = CompletionItemKind.Keyword,
					Label = "else",
					InsertText = "else",
					InsertTextFormat = InsertTextFormat.Snippet,
					Documentation = new MarkupContent()
					{
						Kind = MarkupKind.Markdown,
						Value = "The `if` and `else` keywords are used for controlling *if* code should run, based on a condition that you give it."
					}
				});
			}

			items.Add(new()
			{
				Kind = CompletionItemKind.Keyword,
				Label = "fun",
				Detail = "fun <name>()",
				InsertText = "fun ${1:functionName}($2)${3:: void}\n{\n\t$0\n}",
				InsertTextFormat = InsertTextFormat.Snippet,
				Documentation = new MarkupContent()
				{
					Kind = MarkupKind.Markdown,
					Value = "The `fun` keyword is used to declared a new function."
				}
			});

			items.Add(new()
			{
				Label = "while",
				Detail = "while (...)",
				InsertText = "while ($1) $0",
				InsertTextFormat = InsertTextFormat.Snippet,
				Documentation = new MarkupContent()
				{
					Kind = MarkupKind.Markdown,
					Value = "The `while` keyword is used for repeatedly executing a block of code, as long as a condition you give it continues to be true."
				}
			});

			if (IsInFunction(target, out IConcreteFunctionDeclarationStatementSyntax? function))
			{
				bool hasReturn = (function as ISemanticFunctionDeclarationStatementSyntax)?.Signature.Return?.Type.IsNotVoid ?? function.Signature.Return is not null;

				items.Add(new()
				{
					Kind = CompletionItemKind.Keyword,
					Label = hasReturn ? "return <value>" : "return",
					InsertText = hasReturn ? "return $0;" : "return;",
					InsertTextFormat = InsertTextFormat.Snippet,
					Documentation = new MarkupContent()
					{
						Kind = MarkupKind.Markdown,
						Value = "The `return` keyword is used to return from a function *(optionally with a value).*"
					}
				});
			}
		}
		#endregion

		#region Helpers
		private static bool IsInFunction(ISyntaxToken target, [NotNullWhen(true)] out IConcreteFunctionDeclarationStatementSyntax? function)
		{
			function = target.GetChain().OfType<IConcreteFunctionDeclarationStatementSyntax>().FirstOrDefault();
			return function is not null;
		}
		private static bool IsStatement(ISyntaxToken target)
		{
			bool isEndOfStatement =
				target.Kind == SyntaxKind.Semicolon ||
				target.Kind == SyntaxKind.OpenBrace ||
				target.Kind == SyntaxKind.CloseBrace ||
				target.Kind == SyntaxKind.CloseBracket;

			if (isEndOfStatement is false)
				return false;

			if (target.Kind == SyntaxKind.Semicolon && target.Parent is not IConcreteStatementSyntax)
				return false;

			if (target.GetChain().Any(n => n is ISyntaxDocument or IConcreteFunctionDeclarationStatementSyntax) is false)
				return false;

			return true;
		}
		private bool ShouldIgnore(ISyntaxToken token)
		{
			if (token.Kind == SyntaxKind.StringStart || token.Kind == SyntaxKind.StringText)
				return true;

			if (token.Kind == SyntaxKind.Integer || token.Kind == SyntaxKind.IntegerBase)
				return true;

			return false;
		}
		private static ISyntaxToken? SelectTarget(ICodeSyntaxTree tree, Position position)
		{
			LinePosition target = tree.Source.PositionTranslator.Convert(new(position.Line + 1, position.Character + 1), PositionKind.Utf16, PositionKind.Grapheme);
			return SelectTarget(tree, target);
		}
		private static ISyntaxToken? SelectTarget(ICodeSyntaxTree tree, LinePosition position)
		{
			ISyntaxToken? last = null;

			foreach (ISyntaxToken token in tree.Document.Flatten<ISyntaxToken>())
			{
				if (token.IsFabricated)
					continue;

				if (token.Position.Length is 1)
				{
					if (token.Position.End.Position == position)
						return token;
				}
				else if (token.Position.WithoutIndex.Contains(position))
					return token;

				if (token.Position.End.Position > position)
					return last;

				last = token;
			}

			return last;
		}
		#endregion
	}
	#endregion
}
