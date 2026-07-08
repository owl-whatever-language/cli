namespace OwlDomain.ParsingTools.Diagnostics;

/// <summary>
/// 	Represents diagnostic information.
/// </summary>
public interface IDiagnostic
{
	#region Properties
	/// <summary>The provider that created this diagnostic.</summary>
	IDiagnosticProvider Provider { get; }

	/// <summary>The unique id for this type of diagnostic.</summary>
	string Id { get; }

	/// <summary>The kind of the diagnostic.</summary>
	DiagnosticKind Kind { get; }

	/// <summary>An optional stack trace.</summary>
	StackTrace? StackTrace { get; }

	/// <summary>The primary source file that the diagnostic is referencing.</summary>
	ISourceFile? Source { get; }

	/// <summary>The primary position in the source file tha the diagnostic is referencing.</summary>
	/// <remarks>This will only contain line and column information.</remarks>
	PositionRange Position { get; }

	/// <summary>The primary position in the source file tha the diagnostic is referencing.</summary>
	/// <remarks>This will contain the line, column and index information.</remarks>
	IndexedPositionRange IndexedPosition { get; }

	/// <summary>The short explanation for the diagnostic.</summary>
	/// <remarks>This will likely be the first line of the <see cref="FullMessage"/>.</remarks>
	ITextFragmentLine ShortMessage { get; }

	/// <summary>The full explanation message for the diagnostic.</summary>
	ITextFragmentLineCollection FullMessage { get; }

	/// <summary>A collection of annotations about the code that the diagnostic provides.</summary>
	/// <remarks>The primary message and location will likely be the first </remarks>
	IDiagnosticAnnotationCollection Annotations { get; }

	/// <summary>A collection of all of the source files that are relevant to the diagnostic.</summary>
	IReadOnlySet<ISourceFile> RelevantSources { get; }

	/// <summary>A collection of all of the syntax nodes that are relevant to the diagnostic.</summary>
	IReadOnlySet<ISyntaxNode> RelevantNodes { get; }
	#endregion
}

/// <summary>
/// 	Represents diagnostic information.
/// </summary>
public sealed class Diagnostic : IDiagnostic
{
	#region Properties
	public IDiagnosticProvider Provider { get; }
	public string Id { get; }
	public DiagnosticKind Kind { get; }
	public StackTrace? StackTrace { get; }
	public ISourceFile? Source => Annotations.FirstOrDefault(a => a.Source is not null)?.Source;
	public PositionRange Position => Annotations.FirstOrDefault(a => a.Position != default)?.Position ?? default;
	public IndexedPositionRange IndexedPosition => Annotations.FirstOrDefault(a => a.IndexedPosition != default)?.IndexedPosition ?? default;
	public ITextFragmentLine ShortMessage => FullMessage.First();
	public ITextFragmentLineCollection FullMessage => Annotations.First().Message;
	public DiagnosticAnnotationCollection Annotations { get; } = [];
	public IReadOnlySet<ISourceFile> RelevantSources => Annotations.Where(a => a.Source is not null).Select(a => a.Source).ToHashSet()!;
	public IReadOnlySet<ISyntaxNode> RelevantNodes => Annotations.Where(a => a.Node is not null).Select(a => a.Node).ToHashSet()!;

	IDiagnosticAnnotationCollection IDiagnostic.Annotations => Annotations;
	#endregion

	#region Constructors
	public Diagnostic(IDiagnosticProvider provider, string id, DiagnosticKind kind, StackTrace? stackTrace = null)
	{
		Provider = provider;
		Id = id;
		Kind = kind;
		StackTrace = stackTrace;
	}
	#endregion

	#region Regular add methods
	public Diagnostic Add(TextFragmentLineCollection message)
	{
		DiagnosticAnnotation annotation = new(message);
		return Add(annotation);
	}
	public Diagnostic Add(ISyntaxNode node, TextFragmentLineCollection message)
	{
		DiagnosticAnnotation annotation = new(node, message);
		return Add(annotation);
	}
	public Diagnostic Add(ISourceFile source, TextFragmentLineCollection message)
	{
		DiagnosticAnnotation annotation = new(source, message);
		return Add(annotation);
	}
	public Diagnostic Add(ISourceFile source, PositionRange position, TextFragmentLineCollection message)
	{
		DiagnosticAnnotation annotation = new(source, position, message);
		return Add(annotation);
	}
	public Diagnostic Add(ISourceFile source, LinePosition position, TextFragmentLineCollection message)
	{
		DiagnosticAnnotation annotation = new(source, position, message);
		return Add(annotation);
	}
	public Diagnostic Add(ISourceFile source, IndexedLinePosition position, TextFragmentLineCollection message)
	{
		DiagnosticAnnotation annotation = new(source, position, message);
		return Add(annotation);
	}
	public Diagnostic Add(ISourceFile source, IndexedPositionRange position, TextFragmentLineCollection message)
	{
		DiagnosticAnnotation annotation = new(source, position, message);
		return Add(annotation);
	}
	#endregion

	#region Line builder add methods
	public Diagnostic Add(Action<TextFragmentLineCollection> message)
	{
		DiagnosticAnnotation annotation = new(message);
		return Add(annotation);
	}
	public Diagnostic Add(ISyntaxNode node, Action<TextFragmentLineCollection> message)
	{
		DiagnosticAnnotation annotation = new(node, message);
		return Add(annotation);
	}
	public Diagnostic Add(ISourceFile source, Action<TextFragmentLineCollection> message)
	{
		DiagnosticAnnotation annotation = new(source, message);
		return Add(annotation);
	}
	public Diagnostic Add(ISourceFile source, PositionRange position, Action<TextFragmentLineCollection> message)
	{
		DiagnosticAnnotation annotation = new(source, position, message);
		return Add(annotation);
	}
	public Diagnostic Add(ISourceFile source, LinePosition position, Action<TextFragmentLineCollection> message)
	{
		DiagnosticAnnotation annotation = new(source, position, message);
		return Add(annotation);
	}
	public Diagnostic Add(ISourceFile source, IndexedLinePosition position, Action<TextFragmentLineCollection> message)
	{
		DiagnosticAnnotation annotation = new(source, position, message);
		return Add(annotation);
	}
	public Diagnostic Add(ISourceFile source, IndexedPositionRange position, Action<TextFragmentLineCollection> message)
	{
		DiagnosticAnnotation annotation = new(source, position, message);
		return Add(annotation);
	}
	#endregion

	#region Helpers
	private Diagnostic Add(DiagnosticAnnotation annotation)
	{
		if (Annotations.Count is 0)
		{
			// Note(Nightowl):
			// Only use the diagnostic classification for the first line of the first annotation;
			// Otherwise there would probably be a lot of intimidating red all over.

			if (annotation.Message.Count > 0)
				annotation.Message[0].Replace(ReplaceWithDiagnosticClassification);
		}

		Annotations.Add(annotation);

		return this;
	}
	private TextFragment ReplaceWithDiagnosticClassification(TextFragment fragment)
	{
		if (fragment.Classification is not null)
			return fragment;

		return new(fragment.Text, Kind.ToClassification(), fragment.Syntax);
	}
	#endregion
}
