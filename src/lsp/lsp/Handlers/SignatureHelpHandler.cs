using EmmyLua.LanguageServer.Framework.Protocol.Message.SignatureHelp;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Callable;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Members;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Annotated.Expressions;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Semantic.Expressions;
using OwlDomain.ParsingTools.Positioning;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class SignatureHelpHandler(ILspContext context) : SignatureHelpHandlerBase
{
	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.SignatureHelpProvider = new()
		{
			TriggerCharacters = ["(", ","],
		};
	}
	protected override Task<SignatureHelp> Handle(SignatureHelpParams request, CancellationToken token)
	{
		SignatureHelp result = new() { Signatures = [] };

		if (_context.TryGetTree(request.TextDocument, out ICodeSyntaxTree? tree, out ISourceFile? file) is false)
			return Task.FromResult(result);

		var target = tree.Document.Search<IAnnotatedFunctionCallExpressionSyntax>(request.Position);
		if (target is null)
			return Task.FromResult(result);

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

		LinePosition targetPosition = new(request.Position.Line + 1, request.Position.Character + 1);
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

		return Task.FromResult(result);
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
