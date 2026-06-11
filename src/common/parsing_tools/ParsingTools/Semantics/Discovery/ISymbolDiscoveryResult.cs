namespace OwlDomain.ParsingTools.Semantics.Discovery;

/// <summary>
/// 	Represents the result of the symbol discovery stage.
/// </summary>
public interface ISymbolDiscoveryResult : IStageResult
{
	#region Properties
	/// <summary>The scope for the discovered symbols.</summary>
	ISymbolScope Symbols { get; }

	/// <summary>The collection of the new symbol targets that have been created.</summary>
	/// <remarks>This should be used for ensuring that all of the targets have been locked when the semantic resolution stage is finished.</remarks>
	IReadOnlyCollection<ISymbolTarget> Targets { get; }
	#endregion
}

/// <summary>
/// 	Represents the result of the symbol discovery stage.
/// </summary>
public sealed class SymbolDiscoveryResult : StageResult, ISymbolDiscoveryResult
{
	#region Properties
	/// <inheritdoc/>
	public override string Name => "symbol_discovery";

	/// <inheritdoc/>
	public ISymbolScope Symbols { get; }

	/// <inheritdoc/>
	public IReadOnlyCollection<ISymbolTarget> Targets { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="SymbolDiscoveryResult"/> instance.</summary>
	/// <param name="diagnostics">The diagnostics that occurred during the stage.</param>
	/// <param name="duration">The amount of time it took for the stage to finish processing.</param>
	/// <param name="symbols">The scope for the discovered symbols.</param>
	/// <param name="targets">The collection of the new symbol targets that have been created.</param>
	public SymbolDiscoveryResult(
		IDiagnosticBag diagnostics,
		TimeSpan duration,
		ISymbolScope symbols,
		IReadOnlyCollection<ISymbolTarget> targets)
		: base(diagnostics, duration)
	{
		Symbols = symbols;
		Targets = targets;
	}
	#endregion
}
