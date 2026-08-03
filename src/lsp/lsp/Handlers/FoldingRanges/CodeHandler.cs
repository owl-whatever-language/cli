using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Expressions;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Statements;
using OwlDomain.ParsingTools.Trivia;

namespace OwlDomain.Owl.LSP.Handlers.FoldingRanges;

partial class FoldingRangeHandler
{
	#region Nested types
	private sealed class CodeHandler : BaseCustomTreeHandler<FoldingRangeParams, FoldingRangeResponse, ICodeSyntaxTree>
	{
		#region Methods
		protected override FoldingRangeResponse? Handle(HandlerRequest<FoldingRangeParams> request, ICodeSyntaxTree tree, CancellationToken cancellation)
		{
			List<FoldingRange> ranges = [];
			FoldingRangeResponse response = new(ranges);

			foreach (var block in tree.Document.Flatten<IConcreteBlockStatementSyntax>())
				TryAdd(ranges, block.Start, block.End);

			foreach (var declaration in tree.Document.Flatten<IConcreteFunctionDeclarationStatementSyntax>())
				TryAdd(ranges, declaration.Signature.Start, declaration.Signature.End);

			foreach (var call in tree.Document.Flatten<IConcreteFunctionCallExpressionSyntax>())
				TryAdd(ranges, call.Start, call.End);

			foreach (ISyntaxToken token in tree.Document.Flatten<ISyntaxToken>())
			{
				if (token.LeadingTrivia.All(t => t.Kind == SyntaxKind.Indentation || t.Kind == SyntaxKind.WhiteSpace || t.Kind == SyntaxKind.Comment || t.Kind == SyntaxKind.LineBreak) is false)
					continue;

				ISyntaxTrivia? start = token.LeadingTrivia.FirstOrDefault(t => t.Kind == SyntaxKind.Comment);
				ISyntaxTrivia? last = token.LeadingTrivia.LastOrDefault(t => t.Kind == SyntaxKind.Comment);

				if (start is null || last is null)
					continue;

				TryAdd(ranges, start, last, FoldingRangeKind.Comment);
			}

			return response;
		}
		#endregion
	}
	#endregion
}
