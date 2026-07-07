namespace OwlDomain.ParsingTools.Syntax;

public interface ISyntaxTree : IDebugTreeFactory
{
	#region Properties
	string Kind { get; }
	int Level { get; }
	ISourceFile Source { get; }
	ISyntaxDocument Document { get; }
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public abstract class BaseSyntaxTree<TDocument> : ISyntaxTree
	where TDocument : notnull, ISyntaxDocument
{
	#region Properties
	public abstract string Kind { get; }
	public abstract int Level { get; }
	public ISourceFile Source { get; }
	public TDocument Document { get; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	ISyntaxDocument ISyntaxTree.Document => Document;
	#endregion

	#region Constructors
	protected BaseSyntaxTree(ISourceFile source, TDocument document)
	{
		Source = source;
		Document = document;

		document.Tree = this;
	}
	#endregion

	#region Methods
	public virtual IDebugTree GetDebugTree()
	{
		DebugTree tree = new(this);

		tree.Add(nameof(Kind), Kind);
		tree.Add(nameof(Source), Source.SimpleName, ClassificationKind.File);
		tree.Add(nameof(Document), Document);

		return tree;
	}
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"{GetType().Name}({Source.SimpleName})";
	#endregion
}
