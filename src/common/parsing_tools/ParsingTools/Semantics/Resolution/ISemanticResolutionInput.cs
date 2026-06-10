namespace OwlDomain.ParsingTools.Semantics.Resolution;

/// <summary>
/// 	Represents the additional inputs necessary for the final semantic resolution stage.
/// </summary>
public interface ISemanticResolutionInput
{
	#region Properties
	/// <summary>The symbols available for the semantic resolution.</summary>
	ISymbolScope Symbols { get; }
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
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="BaseSemanticResolutionInput"/> instance.</summary>
	/// <param name="symbols">The symbols available for the semantic resolution.</param>
	public BaseSemanticResolutionInput(ISymbolScope symbols)
	{
		Symbols = symbols;
	}
	#endregion
}
