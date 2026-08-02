namespace OwlDomain.Owl.LSP;

using LspPosition = Position;
using LspRange = DocumentRange;
using OwlPosition = ParsingTools.Positioning.LinePosition;
using OwlPositionRange = ParsingTools.Positioning.Ranges.PositionRange;

public static class LspExtensions
{
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
		public bool TryGetLocation(out Location location)
		{
			if (TryGetLocation(node.GetSource(), node.ToLspPosition, out location))
				return true;

			location = default;
			return false;
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
	extension(ISourceFile source)
	{
		#region Methods
		public bool TryGetUri([NotNullWhen(true)] out Uri? uri)
		{
			if (source.Path is not null)
			{
				// Note(Nightowl): This should keep the file properly as LSP URIs are file://;
				uri = new(source.Path);
				return true;
			}

			uri = default;
			return false;
		}
		public bool TryGetUri(out DocumentUri documentUri)
		{
			if (TryGetUri(source, out Uri? uri))
			{
				documentUri = new(uri);
				return true;
			}

			documentUri = default;
			return false;
		}
		public bool TryGetLocation(LspRange range, out Location location)
		{
			if (TryGetUri(source, out DocumentUri uri))
			{
				location = new(uri, range);
				return true;
			}

			location = default;
			return false;
		}
		public bool TryGetLocation(ISyntaxNode node, out Location location)
		{
			if (TryGetLocation(source, node.ToLspPosition, out location))
				return true;

			location = default;
			return false;
		}
		#endregion
	}

	extension(Uri uri)
	{
		#region Properties
		public string SourcePath => uri.AbsolutePath;
		#endregion
	}
	extension(DocumentUri uri)
	{
		#region Properties
		public string SourcePath => uri.Uri.SourcePath;
		#endregion
	}
	extension(TextDocumentItem document)
	{
		#region Properties
		public string SourcePath => document.Uri.SourcePath;
		#endregion
	}
	extension(TextDocumentIdentifier document)
	{
		#region Properties
		public string SourcePath => document.Uri.SourcePath;
		#endregion
	}
}
