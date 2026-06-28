namespace OwlDomain.ParsingTools.Syntax.Debugging;

public interface IDebugTreePrintable
{
	#region Methods
	/// <summary>Gets the text fragments that make up the symbol signature for debugging purposes.</summary>
	/// <returns>A collection of the text fragments that make up the symbol.</returns>
	TextFragmentCollection GetFragments();
	#endregion
}
