using EmmyLua.LanguageServer.Framework.Protocol.Message.SelectionRange;

namespace OwlDomain.Owl.LSP.Handlers.SelectionRanges;

partial class SelectionRangeHandler
{
	#region Nested types
	private sealed class CodeHandler : BaseCustomTreeHandler<SelectionRangeParams, SelectionRangeResponse, ICodeSyntaxTree>
	{
		#region Methods
		protected override SelectionRangeResponse? Handle(HandlerRequest<SelectionRangeParams> request, ICodeSyntaxTree tree, CancellationToken cancellation)
		{
			List<SelectionRange> ranges = [];
			SelectionRangeResponse response = new(ranges);

			foreach (Position target in request.Request.Positions)
			{
				ISyntaxPart? part = tree.Document.Search<ISyntaxPart>(target);
				if (part is not null)
					ranges.Add(GetRange(part));
				else
					ranges.Add(new());
			}

			return response;
		}
		#endregion
	}
	#endregion
}