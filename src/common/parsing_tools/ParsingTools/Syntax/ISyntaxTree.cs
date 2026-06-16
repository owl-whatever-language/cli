namespace OwlDomain.ParsingTools.Syntax;

public interface ISyntaxTree
{
	#region Properties
	ISourceFile Source { get; }
	#endregion
}

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
}
