namespace OwlDomain.Owl.Code.CodeAnalysis.Diagnostics;

public sealed class CompilationDiagnosticLocation : IDiagnosticLocation
{
	#region Properties
	public static CompilationDiagnosticLocation Instance { get; } = new();
	public ISourceFile? Source => default;
	public PositionRange Position => default;
	public IndexedPositionRange IndexedPosition => default;
	#endregion
}
