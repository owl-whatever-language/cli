namespace OwlDomain.Owl.LSP;

using LspPosition = EmmyLua.LanguageServer.Framework.Protocol.Model.Position;
using LspRange = EmmyLua.LanguageServer.Framework.Protocol.Model.DocumentRange;
using OwlIndexedPosition = ParsingTools.Positioning.IndexedLinePosition;
using OwlIndexedPositionRange = ParsingTools.Positioning.Ranges.IndexedPositionRange;
using OwlPosition = ParsingTools.Positioning.LinePosition;
using OwlPositionRange = ParsingTools.Positioning.Ranges.PositionRange;

internal interface ILspContext
{
	#region Properties
	LanguageServer Server { get; }
	IAnalysisContext Analysis { get; }
	AnalysisUpdateResult? LastAnalysis { get; }
	#endregion

	#region Methods
	ISyntaxTreeBundle? GetBundle(Uri uri);
	Uri? GetUri(ISourceFile source);
	void AddFile(Uri uri, string text);
	void UpdateFile(Uri uri, string text);
	void RemoveFile(Uri uri);
	#endregion
}

internal sealed class LspContext : ILspContext
{
	#region Fields
	private readonly Dictionary<string, WorkspaceSourceFile> _files = [];
	#endregion

	#region Properties
	public LanguageServer Server { get; }
	public AnalysisContext Analysis { get; }
	public AnalysisUpdateResult? LastAnalysis { get; set; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	IAnalysisContext ILspContext.Analysis => Analysis;
	#endregion

	#region Constructors
	public LspContext(LanguageServer server, AnalysisContext analysis)
	{
		Server = server;
		Analysis = analysis;
	}
	#endregion

	#region Methods
	public void RemoveFile(Uri uri)
	{
		lock (Analysis)
		{
			if (_files.Remove(uri.AbsolutePath, out WorkspaceSourceFile? file))
				LastAnalysis = Analysis.Update(removed: [file]);
		}
	}
	public void AddFile(Uri uri, string text)
	{
		lock (Analysis)
		{
			WorkspaceSourceFile file = new(uri, text);
			_files.Add(uri.AbsolutePath, file);
			LastAnalysis = Analysis.Update(added: [file]);
		}
	}
	public void UpdateFile(Uri uri, string text)
	{
		lock (Analysis)
		{
			if (_files.TryGetValue(uri.AbsolutePath, out WorkspaceSourceFile? file))
			{
				file.Text = text;
				LastAnalysis = Analysis.Update(changed: [file]);
			}
		}
	}
	public ISyntaxTreeBundle? GetBundle(Uri uri)
	{
		lock (Analysis)
		{
			if (_files.TryGetValue(uri.AbsolutePath, out WorkspaceSourceFile? file) is false)
				return null;

			if (Analysis.TryGet(file, out ISyntaxTreeBundle? bundle) is false)
				return null;

			return bundle;
		}
	}

	public Uri? GetUri(ISourceFile source)
	{
		lock (Analysis)
		{
			foreach (KeyValuePair<string, WorkspaceSourceFile> file in _files)
			{
				if (file.Value == source)
					return new(file.Key, UriKind.Absolute);
			}

			return null;
		}
	}
	#endregion
}

internal static class ILspContextExtensions
{

	extension(ILspContext context)
	{
		#region Methods
		public bool TryGetBundle(Uri uri, [NotNullWhen(true)] out ISyntaxTreeBundle? bundle)
		{
			bundle = context.GetBundle(uri);
			return bundle is not null;
		}
		public bool TryGetUri(ISourceFile source, [NotNullWhen(true)] out Uri? uri)
		{
			uri = context.GetUri(source);
			return uri is not null;
		}
		#endregion
	}
	extension(LspPosition position)
	{
		#region Properties
		public OwlPosition ToOwl => new(position.Line + 1, position.Character + 1);
		#endregion
	}
	extension(LspRange range)
	{
		#region Properties
		public OwlPositionRange ToOwl => new(range.Start.ToOwl, range.End.ToOwl);
		#endregion
	}
	extension(OwlPosition position)
	{
		#region Properties
		public LspPosition ToLsp => new(position.Line - 1, position.Column - 1);
		#endregion
	}
	extension(OwlIndexedPosition position)
	{
		#region Properties
		public LspPosition ToLsp => new(position.Line - 1, position.Column - 1);
		#endregion
	}
	extension(OwlPositionRange range)
	{
		#region Properties
		public LspRange ToLsp => new(range.Start.ToLsp, range.End.ToLsp);
		#endregion
	}
	extension(OwlIndexedPositionRange range)
	{
		#region Properties
		public LspRange ToLsp => new(range.Start.ToLsp, range.End.ToLsp);
		#endregion
	}
}
