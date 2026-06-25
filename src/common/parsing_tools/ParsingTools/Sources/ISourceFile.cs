namespace OwlDomain.ParsingTools.Sources;

/// <summary>
/// 	Represents information about a source file.
/// </summary>
public interface ISourceFile : IDebugTreePrintable
{
	#region Properties
	/// <summary>The simple name for the source file.</summary>
	string SimpleName { get; }

	/// <summary>The full path to the source file.</summary>
	/// <remarks>This might be <see langword="null"/> if the source file only exists in memory.</remarks>
	string? Path { get; }
	#endregion

	#region Methods
	/// <summary>Creates a general text parser for the source file.</summary>
	/// <returns>A text parser for the source file.</returns>
	ITextParser CreateParser();
	#endregion
}
