namespace OwlDomain.ParsingTools.Syntax;

public interface ISyntaxTree
{
	#region Properties
	ISourceFile Source { get; }
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public abstract class BaseSyntaxTree : ISyntaxTree
{
	#region Properties
	public ISourceFile Source { get; }
	#endregion

	#region Constructors
	protected BaseSyntaxTree(ISourceFile source)
	{
		Source = source;
	}
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"{GetType().Name}({Source.SimpleName})";
	#endregion
}
