using EmmyLua.LanguageServer.Framework.Protocol.Message.SignatureHelp;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Markup;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Functions;
using OwlDomain.Owl.Code.CodeAnalysis.Semantics.Types.Callable;
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
			TriggerCharacters = ["("],
			RetriggerCharacters = [","]
		};
	}
	protected override Task<SignatureHelp> Handle(SignatureHelpParams request, CancellationToken token)
	{
		SignatureHelp result = new() { Signatures = [] };

		if (_context.TryGetBundle(request.TextDocument.Uri.Uri, out ISyntaxTreeBundle? bundle) is false || bundle.LeastDetailed is null)
			return Task.FromResult(result);

		bool IsTarget(ISyntaxNode node)
		{
			LinePosition targetPosition = request.Position.ToOwl;

			if (node.Position.WithoutIndex.Start == targetPosition)
				return true;

			if (node.Position.WithoutIndex.End == targetPosition)
				return true;

			if (node.Position.WithoutIndex.Contains(targetPosition))
				return true;

			return false;
		}

		var target = bundle.LeastDetailed.Document.Search<IAnnotatedFunctionCallExpressionSyntax>(IsTarget);
		if (target is null)
			return Task.FromResult(result);

		if (target.Expression is ISemanticGetExpressionSyntax get)
		{
			foreach (ISymbol candidate in get.Candidates)
			{
				if (candidate is IFunction function)
					result.Signatures.Add(GetSignature(function));
				else if (candidate is ICallableType callable)
					result.Signatures.Add(GetSignature(callable));
			}
		}

		return Task.FromResult(result);
	}
	#endregion

	#region Helpers
	private SignatureInformation GetSignature(IFunction function)
	{
		SignatureInformation signature = new()
		{
			Label = function.GetDebugText().ToPlainText(),
			Parameters = []
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
	private SignatureInformation GetSignature(ICallableType callable)
	{
		SignatureInformation signature = new()
		{
			Label = callable.GetDebugText().ToPlainText(),
			Parameters = []
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
