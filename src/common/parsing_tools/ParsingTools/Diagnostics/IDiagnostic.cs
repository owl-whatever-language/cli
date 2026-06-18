namespace OwlDomain.ParsingTools.Diagnostics;

/// <summary>
/// 	Represents diagnostic information.
/// </summary>
public interface IDiagnostic
{
	#region Properties
	/// <summary>The unique id for this type of diagnostic.</summary>
	string Id { get; }

	/// <summary>The kind of the diagnostic.</summary>
	DiagnosticKind Kind { get; }

	/// <summary>The provider that created this diagnostic.</summary>
	IDiagnosticProvider Provider { get; }

	/// <summary>The location that the diagnostic is for.</summary>
	IDiagnosticLocation Location { get; }

	/// <summary>The diagnostic message.</summary>
	string Message { get; }

	/// <summary>An optional stack trace.</summary>
	StackTrace? StackTrace { get; }
	#endregion
}

/// <summary>
/// 	Represents diagnostic information.
/// </summary>
public sealed class Diagnostic : IDiagnostic
{
	#region Properties
	/// <inheritdoc/>
	public required string Id { get; init; }

	/// <inheritdoc/>
	public required DiagnosticKind Kind { get; init; }

	/// <inheritdoc/>
	public required IDiagnosticProvider Provider { get; init; }

	/// <inheritdoc/>
	public required IDiagnosticLocation Location { get; init; }

	/// <inheritdoc/>
	public required string Message { get; init; }

	/// <inheritdoc/>
	public StackTrace? StackTrace { get; init; }
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override string ToString() => Id;
	#endregion
}
