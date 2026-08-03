using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;
using OwlDomain.Owl.Code.CodeAnalysis.Parsing;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions.Callable;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Loops;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Callable;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Members;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Annotated;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Expressions;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Statements;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Declared.Nodes;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Semantic.Expressions;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Semantic.Statements;
using OwlDomain.ParsingTools.Positioning;
using OwlDomain.ParsingTools.Syntax.Printing;
using OwlDomain.ParsingTools.Trivia;

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
			if (TryAddForAccess(items, target))
				return;

			TryAddKeyword(items, target);
			TryAddForFunctionCall(items, target);

			if (TryAddForExpressionContinuation(items, target) && target.Kind != SyntaxKind.Identifier)
				return;

			TryAddFromScope(items, target);

			items.Add(new()
			{
				Kind = CompletionItemKind.Value,
				Label = "\"\"",
				InsertText = "\"$0\"",
				InsertTextFormat = InsertTextFormat.Snippet,
				Detail = "string",
			});

			items.Add(new()
			{
				Kind = CompletionItemKind.Value,
				Label = "$\"\"",
				InsertText = "\\$\"$0\"",
				InsertTextFormat = InsertTextFormat.Snippet,
				Detail = "interpolated string",
			});
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

					IType? type = group.Select(p => p.Type).Distinct().IfSingleOrDefault();

					CompletionItem item = new()
					{
						Kind = CompletionItemKind.Reference,
						Label = group.Key,
						InsertText = $"{group.Key}: ",
						Detail = type?.GetDebugText().ToPlainText(),
						LabelDetails = new() { Detail = "parameter" }
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

		#region Expression methods
		private bool IsEndOfExpressionStatement(ISyntaxToken target)
		{
			if (target.Parent is not IConcreteExpressionSyntax expression)
				return false;

			IConcreteExpressionStatementSyntax? statement = target.GetChain().OfType<IConcreteExpressionStatementSyntax>().FirstOrDefault();
			if (statement is null)
				return false;

			if (statement.Flatten<ISyntaxToken>().LastOrDefault(t => t.IsFabricated is false) == target)
				return true;

			return false;
		}
		private bool TryAddForExpressionContinuation(List<CompletionItem> items, ISyntaxToken target)
		{
			if (SyntaxKind.BinaryOperators.Contains(target.Kind))
				return false;

			if (target.Parent is not IConcreteExpressionSyntax expression)
				return false;

			if (expression is IConcreteFunctionCallExpressionSyntax call && call.Start == target)
				return false;

			if (expression is IConcreteGroupedExpressionSyntax grouped && grouped.Start == target)
				return false;

			if (expression is IConcreteIfStatementSyntax @if && @if.Start == target)
				return false;

			if (expression is IConcreteIfElseStatementSyntax ifElse && ifElse.Start == target)
				return false;

			if (expression is IConcreteWhileStatementSyntax @while && @while.Start == target)
				return false;

			if (IsEndOfExpressionStatement(target))
			{
				items.Add(new()
				{
					Kind = CompletionItemKind.Text,
					Label = "; <new line>",
					InsertText = ";\n",
					Detail = "end statement"
				});
				items.Add(new()
				{
					Kind = CompletionItemKind.Text,
					Label = ";",
					InsertText = ";",
					Detail = "end statement"
				});
			}

			if (expression is ISemanticExpressionSyntax semantic)
			{
				if (semantic.ResultType.IsError || semantic.ResultType.IsVoid)
					return true;

				items.Add(new()
				{
					Kind = CompletionItemKind.Operator,
					Label = ".",
					InsertText = ".",
					Detail = "access",
				});

				if (semantic.ResultType is ICallableType)
				{
					items.Add(new()
					{
						Kind = CompletionItemKind.Operator,
						Label = "()",
						InsertText = "($0)",
						InsertTextFormat = InsertTextFormat.Snippet,
						Detail = "call",
					});
				}

				foreach (IGrouping<OperatorKind, IBinaryOperator> group in semantic.ResultType.BinaryOperators.GroupBy(op => op.Kind).OrderBy(op => op.Key))
				{
					IType? left = group.Select(b => b.Left).Distinct().IfSingleOrDefault();

					items.Add(new()
					{
						Kind = CompletionItemKind.Operator,
						Label = group.Key.Operator,
						InsertText = group.Key.Operator + " ",
						Detail = group.Key.Name,
						LabelDetails = left is null ? null : new() { Detail = $"{left.GetDebugText().ToPlainText()} operator" }
					});

					if (group.Key.CanBeCompound)
					{
						items.Add(new()
						{
							Kind = CompletionItemKind.Operator,
							Label = group.Key.Operator + "=",
							InsertText = group.Key.Operator + "= ",
							Detail = group.Key.Name + " / assign",
							LabelDetails = left is null ? null : new() { Detail = $"{left.GetDebugText().ToPlainText()} operator" }
						});
					}
				}

				return true;
			}

			foreach (OperatorKind op in Enum.GetValues<OperatorKind>())
			{
				items.Add(new()
				{
					Kind = CompletionItemKind.Operator,
					Label = op.Operator,
					InsertText = op.Operator + " ",
					Detail = op.Name
				});

				if (op.CanBeCompound)
				{
					items.Add(new()
					{
						Kind = CompletionItemKind.Operator,
						Label = op.Operator + "=",
						InsertText = op.Operator + "= ",
						Detail = op.Name + " / assign"
					});
				}
			}

			return true;
		}
		#endregion

		#region Scope methods
		private bool TryGetScope(ISyntaxToken target, [NotNullWhen(true)] out ISymbolScope? scope)
		{
			foreach (ISyntaxNode node in target.GetChain())
			{
				if (node is IDeclaredDocumentSyntax document)
				{
					scope = document.Scope;
					return true;
				}

				if (node is not IAnnotatedSyntaxNode annotated)
					continue;

				if (annotated.TryGetDeclaredScope(out IDeclaredSymbolScope? declared))
				{
					scope = declared;
					return true;
				}
			}

			scope = default;
			return false;
		}
		private void TryAddFromScope(List<CompletionItem> items, ISyntaxToken target)
		{
			if (TryGetScope(target, out ISymbolScope? scope) is false)
				return;

			SymbolSearchResult result = scope.GetNamed();
			foreach (IGrouping<string?, ISymbol> group in result.All.GroupBy(s => s.Name))
			{
				if (string.IsNullOrWhiteSpace(group.Key))
					continue;

				ISymbol[] symbols = group.ToArray();
				ClassificationKind? classification = symbols.GetSharedClassification();
				string ambiguityName = classification == ClassificationKind.Function ? "overloads" : "definitions";

				string? detail = group
					.Select(s => s.GetDebugText().ToPlainText())
					.Distinct()
					.IfSingleOrDefault() ??
					classification?.Split().Last().Name;

				items.Add(new()
				{
					Kind = symbols.Select(GetKindForSymbol).Distinct().IfSingleOrDefault(),
					Label = group.Key,
					InsertText = group.Key,
					Detail = detail,
					LabelDetails = symbols.Length > 1 ? new() { Detail = $"{symbols.Length} {ambiguityName}" } : null,
				});
			}
		}
		private CompletionItemKind GetKindForSymbol(ISymbol symbol)
		{
			return symbol switch
			{
				IFunction => CompletionItemKind.Function,
				IFunctionParameter => CompletionItemKind.Variable,
				ILocalVariable => CompletionItemKind.Variable,
				IType => CompletionItemKind.Class,
				ITypeMethod => CompletionItemKind.Method,
				ITypeProperty => CompletionItemKind.Property,
				ILoopLabel => CompletionItemKind.Reference,

				_ => ThrowHelper.ThrowInvalidOperationException<CompletionItemKind>($"Unhandled scope symbol ({symbol})."),
			};
		}
		#endregion

		#region Access methods
		private bool TryAddForAccess(List<CompletionItem> items, ISyntaxToken target)
		{
			if (target.Parent is not IConcreteMemberAccessExpressionSyntax access || target != access.Dot)
				return false;

			if (IsEndOfExpressionStatement(target) && IsInFunction(target, out IConcreteFunctionDeclarationStatementSyntax? function))
			{
				bool hasReturn = (function as ISemanticFunctionDeclarationStatementSyntax)?.Signature.Return?.Type.IsNotVoid ?? function.Signature.Return is not null;

				if (hasReturn)
				{
					string source = access.Expression.GetDebugSource();
					TextEdit edit = new()
					{
						Range = access.ToLspPosition,
						NewText = $"return {source};"
					};

					items.Add(new()
					{
						Kind = CompletionItemKind.Snippet,
						Label = "return",
						Detail = "return <value>;",
						LabelDetails = new() { Detail = "transform expression into a return" },
						TextEdit = new(edit),
					});
				}
			}

			if (access.Expression is ISemanticExpressionSyntax expression && expression.ResultType.IsNotError)
			{
				foreach (IGrouping<string?, ITypeMethod> group in expression.ResultType.Methods.GroupBy(m => m.Name))
				{
					if (string.IsNullOrWhiteSpace(group.Key))
						continue;

					int count = group.Count();

					items.Add(new()
					{
						Kind = CompletionItemKind.Method,
						Label = group.Key,
						InsertText = group.Key,
						LabelDetails = count > 1 ? new() { Detail = $"{count} overloads" } : null,
						Detail = count is 1 ? group.Single().GetDebugText().ToPlainText() : null,
					});
				}

				foreach (IGrouping<string?, ITypeProperty> group in expression.ResultType.Properties.GroupBy(m => m.Name))
				{
					if (string.IsNullOrWhiteSpace(group.Key))
						continue;

					int count = group.Count();

					items.Add(new()
					{
						Kind = CompletionItemKind.Property,
						Label = group.Key,
						InsertText = group.Key,
						Detail = count is 1 ? group.Single().Type.GetDebugText().ToPlainText() : null
					});
				}
			}

			return true;
		}
		#endregion

		#region Keyword methods
		private static bool ShouldAddElse(ISyntaxToken target)
		{
			if ((target.Kind == SyntaxKind.CloseBrace || target.Kind == SyntaxKind.Semicolon) && target.Parent?.Parent is IConcreteIfStatementSyntax or IConcreteIfElseStatementSyntax)
				return true;

			// Note(Nightowl): Can't yet check if the user started to type 'else';
			if (target.Kind == SyntaxKind.Identifier)
				return true;

			return false;
		}
		private void TryAddKeyword(List<CompletionItem> items, ISyntaxToken target)
		{
			if (IsStatement(target) is false)
				return;

			#region Comments
			items.Add(new()
			{
				Kind = CompletionItemKind.Snippet,
				Label = "//",
				InsertText = "// ",
				Detail = "comment",
				LabelDetails = new() { Detail = "inserts a comment" },
			});
			#endregion

			#region If statement
			items.Add(new()
			{
				Kind = CompletionItemKind.Keyword,
				Label = "if",
				Detail = "if (...)",
				InsertText = "if ($1) $0",
				InsertTextFormat = InsertTextFormat.Snippet,
				LabelDetails = new() { Detail = "if statement" },
				Documentation = new MarkupContent()
				{
					Kind = MarkupKind.Markdown,
					Value = "The `if` and `else` keywords are used for controlling *if* code should run, based on a condition that you give it."
				}
			});

			items.Add(new()
			{
				Kind = CompletionItemKind.Keyword,
				Label = "if",
				Detail = "if (...) {...}",
				InsertText = "if ($1)\n{\n\t$0\n}",
				InsertTextFormat = InsertTextFormat.Snippet,
				LabelDetails = new() { Detail = "if statement" },
				Documentation = new MarkupContent()
				{
					Kind = MarkupKind.Markdown,
					Value = "The `if` and `else` keywords are used for controlling *if* code should run, based on a condition that you give it."
				}
			});
			#endregion

			#region If/else branch
			if (ShouldAddElse(target))
			{
				items.Add(new()
				{
					Kind = CompletionItemKind.Keyword,
					Label = "else",
					InsertText = "else",
					InsertTextFormat = InsertTextFormat.Snippet,
					LabelDetails = new() { Detail = "else branch" },
					Documentation = new MarkupContent()
					{
						Kind = MarkupKind.Markdown,
						Value = "The `if` and `else` keywords are used for controlling *if* code should run, based on a condition that you give it."
					}
				});

				items.Add(new()
				{
					Kind = CompletionItemKind.Keyword,
					Label = "else",
					Detail = "else {...}",
					InsertText = "else\n{\n\t$0\n}",
					InsertTextFormat = InsertTextFormat.Snippet,
					LabelDetails = new() { Detail = "else branch" },
					Documentation = new MarkupContent()
					{
						Kind = MarkupKind.Markdown,
						Value = "The `if` and `else` keywords are used for controlling *if* code should run, based on a condition that you give it."
					}
				});

				items.Add(new()
				{
					Kind = CompletionItemKind.Keyword,
					Label = "else if",
					Detail = "else if (...)",
					InsertText = "else if ($1) $0",
					InsertTextFormat = InsertTextFormat.Snippet,
					LabelDetails = new() { Detail = "else if branch" },
					Documentation = new MarkupContent()
					{
						Kind = MarkupKind.Markdown,
						Value = "The `if` and `else` keywords are used for controlling *if* code should run, based on a condition that you give it."
					}
				});

				items.Add(new()
				{
					Kind = CompletionItemKind.Keyword,
					Label = "else if",
					Detail = "else if (...) {...}",
					InsertText = "else if ($1)\n{\n\t$0\n}",
					InsertTextFormat = InsertTextFormat.Snippet,
					LabelDetails = new() { Detail = "else if branch" },
					Documentation = new MarkupContent()
					{
						Kind = MarkupKind.Markdown,
						Value = "The `if` and `else` keywords are used for controlling *if* code should run, based on a condition that you give it."
					}
				});
			}
			#endregion

			#region Function declaration
			items.Add(new()
			{
				Kind = CompletionItemKind.Keyword,
				Label = "fun",
				Detail = "fun <name>(...) {...}",
				InsertText = "fun ${1:functionName}($2)${3:: void}\n{\n\t$0\n}",
				InsertTextFormat = InsertTextFormat.Snippet,
				LabelDetails = new() { Detail = "function declaration" },
				Documentation = new MarkupContent()
				{
					Kind = MarkupKind.Markdown,
					Value = "The `fun` keyword is used to declared a new function."
				}
			});
			items.Add(new()
			{
				Kind = CompletionItemKind.Keyword,
				Label = "fun",
				Detail = "fun <name>(...);",
				InsertText = "fun ${1:functionName}($2)${3:: void};\n",
				InsertTextFormat = InsertTextFormat.Snippet,
				LabelDetails = new() { Detail = "function declaration" },
				Documentation = new MarkupContent()
				{
					Kind = MarkupKind.Markdown,
					Value = "The `fun` keyword is used to declared a new function."
				}
			});
			items.Add(new()
			{
				Kind = CompletionItemKind.Keyword,
				Label = "fun",
				Detail = "fun <name>(...) =>",
				InsertText = "fun ${1:functionName}($2)${3:: void} => $0;",
				InsertTextFormat = InsertTextFormat.Snippet,
				LabelDetails = new() { Detail = "function declaration" },
				Documentation = new MarkupContent()
				{
					Kind = MarkupKind.Markdown,
					Value = "The `fun` keyword is used to declared a new function."
				}
			});
			#endregion

			#region While loop
			items.Add(new()
			{
				Kind = CompletionItemKind.Keyword,
				Label = "while",
				Detail = "while (...)",
				InsertText = "while ($1) $0",
				InsertTextFormat = InsertTextFormat.Snippet,
				LabelDetails = new() { Detail = "while loop" },
				Documentation = new MarkupContent()
				{
					Kind = MarkupKind.Markdown,
					Value = "The `while` keyword is used for repeatedly executing a block of code, as long as a condition you give it continues to be true."
				}
			});

			items.Add(new()
			{
				Kind = CompletionItemKind.Keyword,
				Label = "while",
				Detail = "while (...) {...}",
				InsertText = "while ($1)\n{\n\t$0\n}",
				InsertTextFormat = InsertTextFormat.Snippet,
				LabelDetails = new() { Detail = "while loop" },
				Documentation = new MarkupContent()
				{
					Kind = MarkupKind.Markdown,
					Value = "The `while` keyword is used for repeatedly executing a block of code, as long as a condition you give it continues to be true."
				}
			});
			#endregion

			if (IsInFunction(target, out IConcreteFunctionDeclarationStatementSyntax? function))
			{
				bool hasReturn = (function as ISemanticFunctionDeclarationStatementSyntax)?.Signature.Return?.Type.IsNotVoid ?? function.Signature.Return is not null;

				items.Add(new()
				{
					Kind = CompletionItemKind.Keyword,
					Label = hasReturn ? "return <value>" : "return",
					InsertText = hasReturn ? "return $0;" : "return;",
					InsertTextFormat = InsertTextFormat.Snippet,
					LabelDetails = new() { Detail = "return statement" },
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
			if (target.Parent is IConcreteGetExpressionSyntax get && get.Name == target && get.Parent is IConcreteExpressionStatementSyntax)
				return true;

			// Note(Nightowl): Special case for comments;
			if (target.Parent is IBadSyntaxTrivia && target.Kind == SyntaxKind.Divide)
				return true;

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

			if (target.Kind == SyntaxKind.CloseBracket)
			{
				if (target.Parent is IConcreteWhileStatementSyntax @while && @while.End == target)
					return true;

				if (target.Parent is IConcreteIfStatementSyntax @if && @if.End == target)
					return true;

				if (target.Parent is IConcreteIfElseStatementSyntax ifElse && ifElse.End == target)
					return true;

				return false;
			}

			return true;
		}
		private bool ShouldIgnore(ISyntaxToken token)
		{
			if (token.Kind == SyntaxKind.StringStart || token.Kind == SyntaxKind.StringText)
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
				if (token.IsFabricated || token.Kind == SyntaxKind.EndOfInput)
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
