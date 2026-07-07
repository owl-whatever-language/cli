namespace OwlDomain.ParsingTools.Diagnostics;

/// <summary>
/// 	Represents location information for a diagnostic.
/// </summary>
public interface IDiagnosticLocation
{
	#region Properties
	ISourceFile? Source { get; }
	PositionRange Position { get; }
	IndexedPositionRange IndexedPosition { get; }
	#endregion
}

/// <summary>
/// 	Represents location information for a diagnostic that originated from a source file.
/// </summary>
/// <remarks>This diagnostic location should only be used when the diagnostic is about the specific file itself.</remarks>
public sealed class DiagnosticSourceLocation : IDiagnosticLocation
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly PositionRange _basicPosition;
	#endregion

	#region Properties
	/// <summary>The source file that the diagnostic is for.</summary>
	public ISourceFile Source { get; }

	/// <summary>The indexed position in the source file that the diagnostic is related to.</summary>
	public IndexedPositionRange IndexedPosition { get; }

	/// <summary>The position in the source file that the diagnostic is related to.</summary>
	public PositionRange Position => (IndexedPosition != default) ? IndexedPosition.WithoutIndex : _basicPosition;
	#endregion

	#region Constructors
	/// <summary>Creates a new diagnostic source location.</summary>
	/// <param name="source">The source file that the diagnostic is for.</param>
	public DiagnosticSourceLocation(ISourceFile source) => Source = source;

	/// <summary>Creates a new diagnostic source location.</summary>
	/// <param name="source">The source file that the diagnostic is for.</param>
	/// <param name="position">The position in the source file that the diagnostic is related to.</param>
	public DiagnosticSourceLocation(ISourceFile source, IndexedPositionRange position)
	{
		Source = source;
		IndexedPosition = position;
		_basicPosition = position.WithoutIndex;
	}

	/// <summary>Creates a new diagnostic source location.</summary>
	/// <param name="source">The source file that the diagnostic is for.</param>
	/// <param name="position">The position in the source file that the diagnostic is related to.</param>
	public DiagnosticSourceLocation(ISourceFile source, PositionRange position)
	{
		Source = source;
		_basicPosition = position;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override string ToString() => $"{Source.SimpleName}, {Position}";
	#endregion
}
