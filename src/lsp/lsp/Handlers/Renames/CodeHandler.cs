using EmmyLua.LanguageServer.Framework.Protocol.Message.Rename;

namespace OwlDomain.Owl.LSP.Handlers.Renames;

partial class RenameHandler
{
	#region Nested types
	private sealed class PrepareCodeHandler : BaseCustomTreeHandler<PrepareRenameParams, PrepareRenameResponse, ICodeSyntaxTree>
	{
		#region Methods
		protected override PrepareRenameResponse? Handle(HandlerRequest<PrepareRenameParams> request, ICodeSyntaxTree tree, CancellationToken cancellation)
		{
			ISyntaxToken? target = tree.Document.Search<ISyntaxToken>(request.Request.Position, true, token => token.Kind == SyntaxKind.Identifier);

			if (target is not null && target.Symbol is IDeclaredSymbol && target.Value is string text)
				return new(target.ToLspPosition, text);

			return null;
		}
		#endregion
	}
	private sealed class CodeHandler : BaseCustomTreeHandler<RenameParams, WorkspaceEdit, ICodeSyntaxTree>
	{
		#region Methods
		protected override WorkspaceEdit? Handle(HandlerRequest<RenameParams> request, ICodeSyntaxTree tree, CancellationToken cancellation)
		{
			ISyntaxToken? target = tree.Document.Search<ISyntaxToken>(request.Request.Position, true, token => token.Kind == SyntaxKind.Identifier);
			if (target is null || target.Symbol is not IDeclaredSymbol symbol)
				return null;

			return GetEdit(request.Workspace, symbol, request.Request.NewName);
		}
		#endregion
	}
	#endregion
}
