namespace OwlDomain.ParsingTools.Parsing.Concrete;

/// <summary>
/// 	Represents a complete concrete syntax tree (CST).
/// </summary>
public interface IConcreteSyntaxTree : ISyntaxTree
{
	#region Properties
	/// <summary>The root document node in the syntax tree.</summary>
	new IConcreteSyntaxNode Document { get; }
	ISyntaxNode ISyntaxTree.Document => Document;
	#endregion
}

/// <summary>
/// 	Represents a complete concrete syntax tree (CST).
/// </summary>
/// <typeparam name="TDocument">The type of the root document node in the syntax tree.</typeparam>
public interface IConcreteSyntaxTree<out TDocument> : IConcreteSyntaxTree, ISyntaxTree<TDocument>
	where TDocument : notnull, IConcreteSyntaxNode
{
	#region Properties
	/// <summary>The root document node in the syntax tree.</summary>
	new TDocument Document { get; }
	IConcreteSyntaxNode IConcreteSyntaxTree.Document => Document;
	ISyntaxNode ISyntaxTree.Document => Document;
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a complete concrete syntax tree (CST).
/// </summary>
/// <typeparam name="TDocument">The type of the root document node in the syntax tree.</typeparam>
public abstract class BaseConcreteSyntaxTree<TDocument> : BaseSyntaxTree<TDocument>, IConcreteSyntaxTree<TDocument>
	where TDocument : notnull, IConcreteSyntaxNode
{
	#region Constructors
	/// <summary>Populates the <see cref="BaseConcreteSyntaxTree{T}"/> properties.</summary>
	/// <param name="source">The source file that the syntax tree represents.</param>
	/// <param name="document">The root document node in the syntax tree.</param>
	protected BaseConcreteSyntaxTree(ISourceFile source, TDocument document) : base(source, document) { }
	#endregion
}
