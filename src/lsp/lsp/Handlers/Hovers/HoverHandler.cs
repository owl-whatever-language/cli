using System.CodeDom.Compiler;
using System.IO;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Hover;

namespace OwlDomain.Owl.LSP.Handlers.Hovers;

internal sealed partial class HoverHandler : HoverHandlerBase
{
	#region Fields
	private readonly CustomTreeHandlerBundle<HoverParams, HoverResponse> _bundle;
	#endregion

	#region Constructors
	public HoverHandler(ILspContext context)
	{
		_bundle = new(context, request => request.TextDocument.SourcePath)
		{
			CodeHandler = new CodeHandler(),
			ConfigHandler = null,
		};
	}
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.HoverProvider = true;
	}
	protected override async Task<HoverResponse?> Handle(HoverParams request, CancellationToken cancellation)
	{
		HoverResponse? response = await _bundle.HandleAsync(request, cancellation);
		if (response is not null && response.Contents.Kind == MarkupKind.Markdown)
		{
			string text = response.Contents.Value;
			text = FixContent(text);
			response.Contents.Value = text;
		}

		if (string.IsNullOrWhiteSpace(response?.Contents.Value))
			response = null;

		return response ?? GetBasicResponse();
	}
	private string FixContent(string content)
	{
		content = content.Trim();
		content = content.RemoveSuffix("\n---", allowMultiple: true);
		content = content.RemovePrefix("---\n", allowMultiple: true);

		if (content == "---")
			content = "";

		return content;
	}
	#endregion

	#region Helpers
	private static HoverResponse ResultFromMarkdown(StringWriter result)
	{
		string text = result.ToString();
		return ResultFromMarkdown(text);
	}
	private static HoverResponse ResultFromMarkdown(string text)
	{
		return new()
		{
			Contents = new()
			{
				Kind = MarkupKind.Markdown,
				Value = text
			}
		};
	}
	private static IndentedTextWriter GetWriter(out StringWriter result, string indent = "  ")
	{
		result = new();
		return new(result, indent);
	}
	private static HoverResponse GetBasicResponse()
	{
		return new()
		{
			Contents = new()
			{
				Kind = MarkupKind.Markdown,
				Value = "*Nothing interesting to show.*"
			}
		};
	}
	#endregion
}

internal static class IndentedTextWriterExtensions
{
	extension(IndentedTextWriter writer)
	{
		#region Methods
		public CustomWriterScope MainSection(string header)
		{
			static void Callback(IndentedTextWriter writer)
			{
				writer.WriteLine();
				writer.WriteLine("---");
				writer.WriteLine();
			}

			writer.MarkdownSection(2, header);
			return new(writer, Callback);
		}
		public CustomWriterScope Documentation() => MainSection(writer, "Documentation");
		public CustomWriterScope Examples() => MainSection(writer, "Examples");
		#endregion
	}
}
