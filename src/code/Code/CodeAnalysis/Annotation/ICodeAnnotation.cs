namespace OwlDomain.Owl.Code.CodeAnalysis.Annotation;

public interface ICodeAnnotation
{
	#region Properties
	string Kind { get; }
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public abstract class CodeAnnotation : ICodeAnnotation
{
	#region Properties
	public abstract string Kind { get; }
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"Annotation: {Kind}";
	#endregion
}
