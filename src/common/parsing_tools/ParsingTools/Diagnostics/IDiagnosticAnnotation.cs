namespace OwlDomain.ParsingTools.Diagnostics;

public interface IDiagnosticAnnotation
{
	#region Properties
	ISourceFile? Source { get; }
	ISyntaxNode? Node { get; }
	IndexedPositionRange IndexedPosition { get; }
	PositionRange Position { get; }
	ITextFragmentLineCollection Message { get; }
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class DiagnosticAnnotation : IDiagnosticAnnotation
{
	#region Properties
	public ISourceFile? Source
	{
		get => field ??= Node?.GetTree().Source;
		set;
	}
	public ISyntaxNode? Node { get; }
	public IndexedPositionRange IndexedPosition
	{
		get => field != default ? field : Node?.Position ?? default;
		set;
	}
	public PositionRange Position
	{
		get
		{
			if (field != default)
				return field;

			IndexedPositionRange indexed = IndexedPosition;
			if (indexed != default)
				return indexed.WithoutIndex;

			return Node?.Position.WithoutIndex ?? default;
		}
		set;
	}
	public TextFragmentLineCollection Message { get; }
	ITextFragmentLineCollection IDiagnosticAnnotation.Message => Message;
	#endregion

	#region Regular constructors
	public DiagnosticAnnotation(TextFragmentLineCollection message)
	{
		Message = message;
	}
	public DiagnosticAnnotation(ISyntaxNode node, TextFragmentLineCollection message)
	{
		Node = node;
		Message = message;
	}
	public DiagnosticAnnotation(ISourceFile source, TextFragmentLineCollection message)
	{
		Source = source;
		Message = message;
	}
	public DiagnosticAnnotation(ISourceFile source, PositionRange position, TextFragmentLineCollection message)
	{
		Source = source;
		Position = position;
		Message = message;
	}
	public DiagnosticAnnotation(ISourceFile source, LinePosition position, TextFragmentLineCollection message)
	{
		Source = source;
		Position = new(position, position);
		Message = message;
	}
	public DiagnosticAnnotation(ISourceFile source, IndexedPositionRange position, TextFragmentLineCollection message)
	{
		Source = source;
		IndexedPosition = position;
		Message = message;
	}
	public DiagnosticAnnotation(ISourceFile source, IndexedLinePosition position, TextFragmentLineCollection message)
	{
		Source = source;
		IndexedPosition = new(position, position);
		Message = message;
	}
	#endregion

	#region Line builder constructors
	public DiagnosticAnnotation(Action<TextFragmentLineCollection> message)
	{
		Message = TextFragment.LineBuilder(message);
	}
	public DiagnosticAnnotation(ISyntaxNode node, Action<TextFragmentLineCollection> message)
	{
		Node = node;
		Message = TextFragment.LineBuilder(message);
	}
	public DiagnosticAnnotation(ISourceFile source, Action<TextFragmentLineCollection> message)
	{
		Source = source;
		Message = TextFragment.LineBuilder(message);
	}
	public DiagnosticAnnotation(ISourceFile source, PositionRange position, Action<TextFragmentLineCollection> message)
	{
		Source = source;
		Position = position;
		Message = TextFragment.LineBuilder(message);
	}
	public DiagnosticAnnotation(ISourceFile source, LinePosition position, Action<TextFragmentLineCollection> message)
	{
		Source = source;
		Position = new(position, position);
		Message = TextFragment.LineBuilder(message);
	}
	public DiagnosticAnnotation(ISourceFile source, IndexedPositionRange position, Action<TextFragmentLineCollection> message)
	{
		Source = source;
		IndexedPosition = position;
		Message = TextFragment.LineBuilder(message);
	}
	public DiagnosticAnnotation(ISourceFile source, IndexedLinePosition position, Action<TextFragmentLineCollection> message)
	{
		Source = source;
		IndexedPosition = new(position, position);
		Message = TextFragment.LineBuilder(message);
	}
	#endregion


	#region Helpers
	private string DebuggerDisplay() => $"Annotation {{ Message = ({Message.ToPlainText()}) }}";
	#endregion
}

public interface IDiagnosticAnnotationCollection : IReadOnlyList<IDiagnosticAnnotation>
{
}

public sealed class DiagnosticAnnotationCollection : List<IDiagnosticAnnotation>, IDiagnosticAnnotationCollection
{
	#region Properties
	public static IDiagnosticAnnotationCollection Empty { get; } = new DiagnosticAnnotationCollection();
	#endregion

	#region Constructors
	public DiagnosticAnnotationCollection() { }
	public DiagnosticAnnotationCollection(IEnumerable<IDiagnosticAnnotation> annotations) : base(annotations) { }
	#endregion
}

public static class IDiagnosticAnnotationExtensions
{
	extension(IDiagnosticAnnotation annotation)
	{
		#region Properties
		public bool IsPositionSpecific
		{
			get
			{
				if (annotation.Position == default)
					return false;

				if (annotation.Node is null or ISyntaxToken)
					return true;

				if (annotation.Position.Start == annotation.Position.End)
				{
					// Note(Nightowl): Pretty much exclusively used for reporting missing syntax, which is position specific;
					return true;
				}

				return false;
			}
		}
		#endregion
	}
}
