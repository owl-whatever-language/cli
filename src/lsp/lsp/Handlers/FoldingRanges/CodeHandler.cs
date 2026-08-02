using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Expressions;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Statements;

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

			return response;
		}
		#endregion
	}
	#endregion
}
