namespace OwlDomain.ParsingTools;

/// <summary>
///   Represents an instance that can do the processing for a single stage operation.
/// </summary>
public abstract class StageInstance
{
	#region Properties
	/// <summary>The diagnostic provider to use when creating new diagnostics for the stage.</summary>
	protected IDiagnosticProvider DiagnosticProvider { get; }

	/// <summary>The diagnostics that have occurred during the processing of the stage.</summary>
	protected DiagnosticBag Diagnostics { get; } = [];
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="StageInstance"/> properties.</summary>
	/// <param name="diagnosticProvider">The diagnostic provider to use when creating new diagnostics for the stage.</param>
	protected StageInstance(IDiagnosticProvider diagnosticProvider)
	{
		DiagnosticProvider = diagnosticProvider;
	}
	#endregion
}
