using EmmyLua.LanguageServer.Framework.Protocol.Message.InlayHint;

namespace OwlDomain.Owl.LSP.Handlers.InlayHints;

partial class InlayHintHandler
{
	#region Nested types
	private sealed class ConfigHandler : BaseCustomTreeHandler<InlayHintParams, InlayHintResponse, IConfigSyntaxTree, InlayHint>
	{
	}
	#endregion
}
