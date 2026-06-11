namespace OwlDomain.ParsingTools.Semantics.Resolution;

/// <summary>
/// 	Represents the additional inputs necessary for the final semantic resolution stage.
/// </summary>
public interface ISemanticResolutionInput
{
	#region Properties
	/// <summary>The symbols available for the semantic resolution.</summary>
	ISymbolScope Symbols { get; }

	/// <summary>The collection of targets that have been created during symbol discovery.</summary>
	/// <remarks>This should be used for ensuring that all of the targets have been locked when the semantic resolution stage is finished.</remarks>
	IReadOnlyCollection<ISymbolTarget> Targets { get; }
	#endregion
}

/// <summary>
/// 	Represents the additional inputs necessary for the final semantic resolution stage.
/// </summary>
public abstract class BaseSemanticResolutionInput : ISemanticResolutionInput
{
	#region Properties
	/// <inheritdoc/>
	public ISymbolScope Symbols { get; }

	/// <inheritdoc/>
	public IReadOnlyCollection<ISymbolTarget> Targets { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="BaseSemanticResolutionInput"/> instance.</summary>
	/// <param name="symbols">The symbols available for the semantic resolution.</param>
	/// <param name="targets">The collection of targets that have been created during symbol discovery.</param>
	public BaseSemanticResolutionInput(ISymbolScope symbols, IReadOnlyCollection<ISymbolTarget> targets)
	{
		Symbols = symbols;
		Targets = targets;
	}
	#endregion
}
