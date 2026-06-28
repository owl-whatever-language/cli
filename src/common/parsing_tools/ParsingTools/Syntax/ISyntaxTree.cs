namespace OwlDomain.ParsingTools.Syntax;

public interface ISyntaxTree
{
	#region Properties
	string Kind { get; }
	int Level { get; }
	ISourceFile Source { get; }
	ISyntaxNode Document { get; }
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public abstract class BaseSyntaxTree<TDocument> : ISyntaxTree
	where TDocument : notnull, ISyntaxNode
{
	#region Properties
	public abstract string Kind { get; }
	public abstract int Level { get; }
	public ISourceFile Source { get; }
	public TDocument Document { get; }
	ISyntaxNode ISyntaxTree.Document => Document;
	#endregion

	#region Constructors
	protected BaseSyntaxTree(ISourceFile source, TDocument document)
	{
		Source = source;
		Document = document;
	}
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"{GetType().Name}({Source.SimpleName})";
	#endregion
}
