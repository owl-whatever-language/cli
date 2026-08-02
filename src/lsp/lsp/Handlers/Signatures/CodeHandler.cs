using EmmyLua.LanguageServer.Framework.Protocol.Message.SignatureHelp;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Callable;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Members;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Annotated.Expressions;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Semantic.Expressions;
using OwlDomain.ParsingTools.Positioning;

namespace OwlDomain.Owl.LSP.Handlers.Signatures;

partial class SignatureHelpHandler
{
	#region Nested types
	private sealed class CodeHandler : BaseCustomTreeHandler<SignatureHelpParams, SignatureHelp, ICodeSyntaxTree>
	{
		#region Methods
		protected override SignatureHelp? Handle(HandlerRequest<SignatureHelpParams> request, ICodeSyntaxTree tree, CancellationToken cancellation)
		{
			var target = tree.Document.Search<IAnnotatedFunctionCallExpressionSyntax>(request.Request.Position);
			if (target is null)
				return null;

			SignatureHelp result = new() { Signatures = [] };

			List<ISymbol> candidates = [];
			ISymbol? chosen = null;

			if (target.Expression is ISemanticGetExpressionSyntax get)
			{
				candidates.AddRange(get.Candidates);
				chosen = get.Symbol;
			}
			else if (target.Expression is ISemanticMemberAccessExpressionSyntax member)
			{
				if (member.Symbol.IsKnown)
				{
					candidates.Add(member.Symbol);
					chosen = member.Symbol;
				}
			}

			LinePosition targetPosition = new(request.Request.Position.Line + 1, request.Request.Position.Character + 1);
			targetPosition = tree.Source.PositionTranslator.Convert(targetPosition, PositionKind.Utf16, PositionKind.Grapheme);

			int commaCount = target.Arguments.Separators.Count(s => s.Position.WithoutIndex.Start < targetPosition);
			foreach (ISymbol candidate in candidates)
			{
				SignatureInformation? signature = candidate switch
				{
					IFunction function => GetSignature(function, commaCount),
					ITypeMethod method => GetSignature(method.Function, commaCount),
					ICallableType callable => GetSignature(callable, commaCount),

					_ => null
				};

				if (signature is not null)
					result.Signatures.Add(signature);
			}

			int? index = chosen is null ? null : candidates.IndexOf(chosen);
			if (index < 0)
				index = null;

			result.ActiveSignature = (uint?)index;

			return result;
		}
		#endregion

		#region Helpers
		private SignatureInformation GetSignature(IFunction function, int commaCount)
		{
			SignatureInformation signature = new()
			{
				Label = function.GetDebugText().ToPlainText(),
				Parameters = [],
				ActiveParameter = (uint)commaCount
			};

			foreach (IFunctionParameter parameter in function.Parameters)
			{
				signature.Parameters.Add(new()
				{
					Label = parameter.Name ?? "_",
				});
			}

			return signature;
		}
		private SignatureInformation GetSignature(ICallableType callable, int commaCount)
		{
			SignatureInformation signature = new()
			{
				Label = callable.GetDebugText().ToPlainText(),
				Parameters = [],
				ActiveParameter = (uint)commaCount
			};

			foreach (ICallableTypeParameter parameter in callable.Parameters)
			{
				signature.Parameters.Add(new()
				{
					Label = parameter.Name ?? "_",
					Documentation = new MarkupContent() { Value = $"```owl\n{parameter.GetDebugText().ToPlainText()}\n```" },
				});
			}

			return signature;
		}
		#endregion
	}
	#endregion
}
