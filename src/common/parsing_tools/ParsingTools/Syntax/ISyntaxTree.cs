namespace OwlDomain.ParsingTools.Syntax;

public interface ISyntaxTree
{
	#region Properties
	string Kind { get; }
	int Level { get; }
	ISourceFile Source { get; }
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public abstract class BaseSyntaxTree : ISyntaxTree
{
	#region Properties
	public abstract string Kind { get; }
	public abstract int Level { get; }
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
