namespace OwlDomain.ParsingTools;

/// <summary>
/// 	Represents the results of a processing stage.
/// </summary>
public interface IStageResult
{
	#region Properties
	/// <summary>The name of the stage.</summary>
	string Name { get; }

	/// <summary>Whether the stage can be considered successful.</summary>
	bool IsSuccessful { get; }

	/// <summary>The diagnostics that occurred during the stage.</summary>
	IDiagnosticBag Diagnostics { get; }

	/// <summary>The amount of time it took for the stage to finish processing.</summary>
	TimeSpan Duration { get; }
	#endregion
}

/// <summary>
/// 	Represents the results of a processing stage.
/// </summary>
public class StageResult : IStageResult
{
	#region Properties
	/// <inheritdoc/>
	public virtual string Name => "unknown";

	/// <inheritdoc/>
	public virtual bool IsSuccessful => Diagnostics.HasErrors is false;

	/// <inheritdoc/>
	public IDiagnosticBag Diagnostics { get; }

	/// <inheritdoc/>
	public TimeSpan Duration { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="StageResult"/> instance.</summary>
	/// <param name="diagnostics">The diagnostics that occurred during the stage.</param>
	/// <param name="duration">The amount of time it took for the stage to finish processing.</param>
	public StageResult(IDiagnosticBag diagnostics, TimeSpan duration)
	{
		Diagnostics = diagnostics;
		Duration = duration;
	}
	#endregion
}
