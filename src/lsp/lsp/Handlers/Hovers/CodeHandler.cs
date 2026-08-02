using System.CodeDom.Compiler;
using System.IO;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Hover;
using OwlDomain.Owl.Code.CodeAnalysis.Parsing;

namespace OwlDomain.Owl.LSP.Handlers.Hovers;

partial class HoverHandler
{
	#region Nested types
	private sealed class CodeHandler : BaseCustomTreeHandler<HoverParams, HoverResponse, ICodeSyntaxTree>
	{
		#region Methods
		protected override HoverResponse? Handle(HandlerRequest<HoverParams> request, ICodeSyntaxTree tree, CancellationToken cancellation)
		{
			ISyntaxNode? original = tree.Document.Search<ISyntaxToken>(request.Request.Position);
			if (original is null)
				return null;

			using (IndentedTextWriter writer = GetWriter(out StringWriter result))
			{
				ISyntaxNode corrected = CorrectTarget(original);
				WriteHover(writer, original, original);

				return ResultFromMarkdown(result);
			}
		}
		private void WriteHover(IndentedTextWriter writer, ISyntaxNode original, ISyntaxNode target)
		{
			WriteDocumentation(writer, original, target);
			WriteExample(writer, original, target);
		}
		private ISyntaxNode CorrectTarget(ISyntaxNode node)
		{
			return node;
		}
		#endregion

		#region Documentation methods
		private static void WriteDocumentation(IndentedTextWriter writer, ISyntaxNode original, ISyntaxNode target)
		{
			if (IsKeyword(original, out ISyntaxToken? keyword))
			{
				WriteDocumentationForKeyword(writer, keyword);
				return;
			}
		}
		private static void WriteDocumentationForKeyword(IndentedTextWriter writer, ISyntaxToken keyword)
		{
			using (writer.Documentation())
			{
				if (keyword.Kind == SyntaxKind.If || keyword.Kind == SyntaxKind.Else)
				{
					writer.WriteLine($"The `if` and `else` keywords are used for controlling *if* code should run, based on a condition that you give it.");
				}
				else if (keyword.Kind == SyntaxKind.While)
				{
					writer.WriteLine($"The `while` keyword is used for repeatedly executing a block of code, as long as a condition you give it continues to be true.");
				}
				else if (keyword.Kind == SyntaxKind.Fun)
				{
					writer.WriteLine($"The `fun` keyword is used to declared a new function.");
				}
				else if (keyword.Kind == SyntaxKind.Return)
				{
					writer.WriteLine($"The `return` keyword is used to return from a function *(optionally with a value).*");
				}
				else
				{
					writer.WriteLine($"*There's no documentation for the `{keyword.Kind.Name}` keyword yet.*<br>");
					writer.WriteLine("Please report this to the developers on [GitHub issues](https://github.com/owl-whatever-language/cli/issues).");
				}
			}
		}
		#endregion

		#region Example methods
		private static void WriteExample(IndentedTextWriter writer, ISyntaxNode original, ISyntaxNode target)
		{
			if (IsKeyword(original, out ISyntaxToken? keyword))
			{
				WriteExampleForKeyword(writer, keyword);
				return;
			}
		}
		private static void WriteExampleForKeyword(IndentedTextWriter writer, ISyntaxToken keyword)
		{
			using (_ = writer.Examples())
			{
				if (keyword.Kind == SyntaxKind.If || keyword.Kind == SyntaxKind.Else)
				{
					writer.MarkdownCode("owl",
					"""
					int value = MyCustomFunction();

					if (value < 10) // The if keyword checks the condition.
					{
						// The code in this block will run if the value is less than 10.
					}
					else // Optionally, you can specify what to do if the condition failed.
					{
						// The code in this block will run if the value is NOT less than 10.
					}
					""");

					writer.WriteLine("You can also chain these `if`/`else` statements together like so:");
					writer.MarkdownCode("owl",
					"""
						int value = MyCustomFunction();

						if (value < 10)
						{
							// Code runs if the value is less than 10.
						}
						else if (value < 100)
						{
							// Code runs if the value is less than 100, but not less than 10.
							// This is because the if statements are checked in order.
						}
						else
						{
							// Code runs if both conditions failed.
							// This means that the value is greater than or equal to 100.
						}
						""");
				}
				else if (keyword.Kind == SyntaxKind.While)
				{
					writer.MarkdownCode("owl",
					"""
					int value = MyCustomFunction();

					while (value > 10) // The while keyword checks the condition each iteration.
					{
						// The code in this block will continue to repeat,
						// as long the value is greater than 10.
						value -= 1;
					}
					""");
				}
				else if (keyword.Kind == SyntaxKind.Fun || keyword.Kind == SyntaxKind.Return)
				{
					writer.MarkdownCode("owl",
					"""
					// The fun keyword declares a new function.
					// This function can optionally take in some parameters.
					fun MyCustomFunction(int value)
					{
						if (value > 10)
						{
							// If the value is greater than 10, then we exit the function.
							return;
						}

						// This code will only run when value is less than or equal to 10.
						DoSomethingWithValue(value);
					}

					// You can then call the function, and provide the value for the parameter.
					MyCustomFunction(5);
					""");

					writer.MarkdownCode("owl",
					"""
					// Functions can also specify a return type.
					// This means that they HAVE to return a value.
					fun MyCustomFunction2(int value): int
					{
						if (value > 10)
						{
							// This is NOT allowed and it will ERROR.
							// A value MUST be specified.
							return;
						}

						// We return the provided value, but we add 5 to it.
						return value + 5;
					}

					int result = MyCustomFunction2(3); // Results in 8.
					""");
				}
				else
				{
					writer.WriteLine($"*There are no examples for the `{keyword.Kind.Name}` keyword yet.*<br>");
					writer.WriteLine("Please report this to the developers on [GitHub issues](https://github.com/owl-whatever-language/cli/issues).");
				}
			}
		}
		#endregion

		#region Helpers
		private static bool IsKeyword(ISyntaxNode node, [NotNullWhen(true)] out ISyntaxToken? keyword)
		{
			if (node is ISyntaxToken token && SyntaxKind.AllKeywords.Contains(token.Kind))
			{
				keyword = token;
				return true;
			}

			keyword = default;
			return false;
		}
		#endregion
	}
	#endregion
}
