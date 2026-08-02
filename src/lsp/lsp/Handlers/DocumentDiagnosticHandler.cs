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

		string path = request.TextDocument.SourcePath;

		if (_context.TryGet(path, out IOwlWorkspace? workspace) is false)
			return Task.FromResult(report);

		if (workspace.IsRelevantFile(path, out ISourceFile? source) is false)
			return Task.FromResult(report);

		IDiagnosticBag? diagnostics = null;

		// Note(Nightowl): Refactor this to instead have the workspace contain all the diagnostics;

		if (workspace.IsCode(path))
			diagnostics = workspace.LastCodeUpdate?.GetAllDiagnostics();
		else if (workspace.IsWorkspace(path))
			diagnostics = workspace.LastWorkspaceUpdate?.GetAllDiagnostics();
		else if (workspace.IsPackage(path))
			diagnostics = workspace.LastPackageUpdate?.GetAllDiagnostics();
		else if (workspace.IsConfig(path))
			diagnostics = workspace.LastConfigUpdate?.GetAllDiagnostics();

		foreach (IDiagnostic current in diagnostics?.Where(d => d.Source == source) ?? [])
		{
			LspDiagnostic diagnostic = GetDiagnostic(current);
			full.Diagnostics.Add(diagnostic);
		}

		return Task.FromResult(report);
	}

	private LspDiagnostic GetDiagnostic(IDiagnostic diagnostic)
	{
		DiagnosticSeverity severity = DiagnosticSeverity.Hint;

		if (diagnostic.Kind >= DiagnosticKind.Error)
			severity = DiagnosticSeverity.Error;
		else if (diagnostic.Kind >= DiagnosticKind.Warning)
			severity = DiagnosticSeverity.Warning;
		else if (diagnostic.Kind >= DiagnosticKind.Suggestion)
			severity = DiagnosticSeverity.Information;

		LspDiagnostic lsp = new()
		{
			Severity = severity,
			Range = diagnostic.ToLspPosition,
			Code = diagnostic.Id,
			Source = "OWL",
			Message = string.Join("\n", diagnostic.FullMessage.ToPlainText()),
			RelatedInformation = [],
		};

		foreach (IDiagnosticAnnotation annotation in diagnostic.Annotations.Skip(1))
		{
			if (annotation.Source?.TryGetLocation(annotation.ToLspPosition, out Location location) is true)
				lsp.RelatedInformation.Add(new(location, annotation.Message.ToPlainText()));
		}

		return lsp;
	}
	#endregion
}
