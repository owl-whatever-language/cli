using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentDiagnostic;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;

namespace OwlDomain.Owl.LSP.Handlers;

using LspDiagnostic = EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic.Diagnostic;

internal sealed class DocumentDiagnosticHandler(ILspContext context) : DocumentDiagnosticHandlerBase
{
	#region Fields
	private readonly ILspContext _context = context;
	#endregion

	#region Methods
	public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
	{
		serverCapabilities.DiagnosticProvider = new()
		{
			InterFileDependencies = true
		};
	}
	protected override Task<DocumentDiagnosticReport> Handle(DocumentDiagnosticParams request, CancellationToken token)
	{
		RelatedFullDocumentDiagnosticReport full = new() { Diagnostics = [] };
		DocumentDiagnosticReport report = new(full);

		if (_context.TryGetWorkspace(request.TextDocument, out ISourceFile? file, out IOwlWorkspace? workspace) is false)
			return Task.FromResult(report);

		foreach (IDiagnostic current in workspace.LastCodeUpdate?.GetAllDiagnostics().Where(d => d.Source == file) ?? [])
		{
			DiagnosticSeverity severity = DiagnosticSeverity.Hint;

			if (current.Kind >= DiagnosticKind.Error)
				severity = DiagnosticSeverity.Error;
			else if (current.Kind >= DiagnosticKind.Warning)
				severity = DiagnosticSeverity.Warning;
			else if (current.Kind >= DiagnosticKind.Suggestion)
				severity = DiagnosticSeverity.Information;

			LspDiagnostic diagnostic = new()
			{
				Severity = severity,
				Range = current.ToLspPosition,
				Code = current.Id,
				Source = "OWL",
				Message = string.Join("\n", current.FullMessage.ToPlainText()),
				RelatedInformation = [],
			};

			foreach (IDiagnosticAnnotation annotation in current.Annotations.Skip(1))
			{
				if (annotation.Source?.TryGetLocation(annotation.ToLspPosition, out Location location) is true)
					diagnostic.RelatedInformation.Add(new(location, annotation.Message.ToPlainText()));
			}

			full.Diagnostics.Add(diagnostic);
		}

		return Task.FromResult(report);
	}
	#endregion
}
