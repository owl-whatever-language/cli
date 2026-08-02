using EmmyLua.LanguageServer.Framework.Protocol.Message.Rename;

namespace OwlDomain.Owl.LSP.Handlers.Renames;

partial class RenameHandler
{
	#region Nested types
	private sealed class PrepareConfigHandler : BaseCustomTreeHandler<PrepareRenameParams, PrepareRenameResponse, IConfigSyntaxTree>
	{
		#region Methods
		protected override PrepareRenameResponse? Handle(HandlerRequest<PrepareRenameParams> request, IConfigSyntaxTree tree, CancellationToken cancellation)
		{
			ISyntaxToken? target = tree.Document.Search<ISyntaxToken>(request.Request.Position, true, token => token.Kind == SyntaxKind.Identifier);

			if (target is not null && target.Symbol is IDeclaredSymbol && target.Value is string text)
				return new(target.ToLspPosition, text);

			return null;
		}
		#endregion
	}
	private sealed class ConfigHandler : BaseCustomTreeHandler<RenameParams, WorkspaceEdit, IConfigSyntaxTree>
	{
		#region Methods
		protected override WorkspaceEdit? Handle(HandlerRequest<RenameParams> request, IConfigSyntaxTree tree, CancellationToken cancellation)
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
