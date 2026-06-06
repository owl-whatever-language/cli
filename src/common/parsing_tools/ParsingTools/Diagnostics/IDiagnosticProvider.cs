namespace OwlDomain.ParsingTools.Diagnostics;

/// <summary>
/// 	Represents a type that can create diagnostics.
/// </summary>
public interface IDiagnosticProvider
{
	#region Properties
	/// <summary>The name of the diagnostic provider.</summary>
	string Name { get; }
	#endregion
}
