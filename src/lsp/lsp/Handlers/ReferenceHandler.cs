using EmmyLua.LanguageServer.Framework.Protocol.Message.Reference;
using EmmyLua.LanguageServer.Framework.Protocol.Model;

namespace OwlDomain.Owl.LSP.Handlers;

internal sealed class ReferenceHandler(ILspContext context) : ReferenceHandlerBase
{
	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.ReferencesProvider = true;
	}
	protected override Task<ReferenceResponse?> Handle(ReferenceParams request, CancellationToken cancellationToken)
	{
		if (_context.TryGetBundle(request.TextDocument.Uri.Uri, out ISyntaxTreeBundle? bundle) is false || bundle.LeastDetailed is null)
			return Task.FromResult<ReferenceResponse?>(null);

		ISyntaxToken? target = bundle.LeastDetailed.Document.Search<ISyntaxToken>(token => token.Position.WithoutIndex.Contains(request.Position.ToOwl));
		if (target is null)
			return Task.FromResult<ReferenceResponse?>(null);

		List<Location> locations = [];

		if (target.Symbol?.IsKnown is true)
		{
			foreach (var current in _context.Analysis.Annotated)
			{
				if (_context.TryGetUri(current.Source, out Uri? uri) is false)
					continue;

				foreach (var token in current.Document.ToTokens())
				{
					if (token.Symbol == target.Symbol)
						locations.Add(new(uri, token.Position.ToLsp));
				}
			}
		}

		return Task.FromResult<ReferenceResponse?>(new(locations));
	}
	#endregion
}
