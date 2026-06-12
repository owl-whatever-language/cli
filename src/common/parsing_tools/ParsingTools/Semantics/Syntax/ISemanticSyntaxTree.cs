namespace OwlDomain.ParsingTools.Semantics.Syntax;

/// <summary>
/// 	Represents a complete semantic syntax tree (SST).
/// </summary>
public interface ISemanticSyntaxTree : IConcreteSyntaxTree
{
	#region Properties
	/// <summary>The root document node in the syntax tree.</summary>
	new ISemanticSyntaxNode Document { get; }
	IConcreteSyntaxNode IConcreteSyntaxTree.Document => Document;
	#endregion
}

/// <summary>
///	Represents a complete semantic syntax tree (SST).
/// </summary>
/// <typeparam name="TDocument">The type of the root document node in the syntax tree.</typeparam>
public interface ISemanticSyntaxTree<out TDocument> : ISemanticSyntaxTree, ISyntaxTree<TDocument>
	where TDocument : notnull, ISemanticSyntaxNode
{
	#region Properties
	/// <summary>The root document node in the syntax tree.</summary>
	new TDocument Document { get; }
	ISemanticSyntaxNode ISemanticSyntaxTree.Document => Document;
	ISyntaxNode ISyntaxTree.Document => Document;
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a complete semantic syntax tree (SST).
/// </summary>
/// <typeparam name="TDocument">The type of the root document node in the syntax tree.</typeparam>
public abstract class BaseSemanticSyntaxTree<TDocument> : BaseSyntaxTree<TDocument>, ISemanticSyntaxTree<TDocument>
	where TDocument : notnull, ISemanticSyntaxNode
{
	#region Constructors
	/// <summary>Populates the <see cref="BaseSemanticSyntaxTree{TDocument}"/> properties.</summary>
	/// <param name="source">The source file that the syntax tree represents.</param>
	/// <param name="document">The root document node in the syntax tree.</param>
	protected BaseSemanticSyntaxTree(ISourceFile source, TDocument document) : base(source, document) { }
	#endregion
}
