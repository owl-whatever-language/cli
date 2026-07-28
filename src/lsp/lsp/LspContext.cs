namespace OwlDomain.Owl.LSP;

using OwlDomain.ParsingTools.Diagnostics;
using LspPosition = EmmyLua.LanguageServer.Framework.Protocol.Model.Position;
using LspRange = EmmyLua.LanguageServer.Framework.Protocol.Model.DocumentRange;
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
	extension(ISyntaxNode node)
	{
		#region Properties
		public LspRange ToLspPosition
		{
			get
			{
				OwlPositionRange zeroIndexed = node.GetTree().Source.PositionTranslator.Convert(node.Position, PositionKind.Grapheme, PositionKind.Utf16);
				LspPosition start = new(zeroIndexed.Start.Line - 1, zeroIndexed.Start.Column - 1);
				LspPosition end = new(zeroIndexed.End.Line - 1, zeroIndexed.End.Column - 1);

				return new(start, end);
			}
		}
		#endregion

		#region Methods
		public ISyntaxNode? Search(LspPosition position, bool includeSelf = true, Predicate<ISyntaxNode>? condition = null)
		{
			return Search<ISyntaxNode>(node, position, includeSelf, condition);
		}
		public T? Search<T>(LspPosition position, bool includeSelf = true, Predicate<T>? condition = null)
			where T : notnull, ISyntaxNode
		{
			OwlPosition target = node.GetTree().Source.PositionTranslator.Convert(new(position.Line + 1, position.Character + 1), PositionKind.Utf16, PositionKind.Grapheme);

			bool Condition(T node)
			{
				if (node.Position.WithoutIndex.Contains(target) is false)
					return false;

				if (condition is not null)
					return condition.Invoke(node);

				return true;
			}

			return node.Search<T>(Condition, includeSelf);
		}
		#endregion
	}
	extension(IDiagnosticAnnotation annotation)
	{
		#region Properties
		public LspRange ToLspPosition
		{
			get
			{
				if (annotation.Source is null)
					return default;

				OwlPositionRange oneIndexed = annotation.Source.PositionTranslator.Convert(annotation.Position, PositionKind.Grapheme, PositionKind.Utf16);
				LspPosition start = new(oneIndexed.Start.Line - 1, oneIndexed.Start.Column - 1);
				LspPosition end = new(oneIndexed.End.Line - 1, oneIndexed.End.Column - 1);

				return new(start, end);
			}
		}
		#endregion
	}
	extension(IDiagnostic diagnostic)
	{
		#region Properties
		public LspRange ToLspPosition => diagnostic.Annotations.FirstOrDefault(a => a.Position != default)?.ToLspPosition ?? default;
		#endregion
	}
}
