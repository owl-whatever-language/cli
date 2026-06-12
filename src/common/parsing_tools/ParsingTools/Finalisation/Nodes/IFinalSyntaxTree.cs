namespace OwlDomain.ParsingTools.Finalisation.Nodes;

/// <summary>
/// 	Represents a complete final syntax tree (FST).
/// </summary>
public interface IFinalSyntaxTree : ISemanticSyntaxTree
{
	#region Properties
	/// <summary>The root document node in the syntax tree.</summary>
	new IFinalSyntaxNode Document { get; }
	ISemanticSyntaxNode ISemanticSyntaxTree.Document => Document;
	#endregion
}

/// <summary>
///	Represents a complete final syntax tree (FST).
/// </summary>
/// <typeparam name="TDocument">The type of the root document node in the syntax tree.</typeparam>
public interface IFinalSyntaxTree<out TDocument> : IFinalSyntaxTree, ISyntaxTree<TDocument>
	where TDocument : notnull, IFinalSyntaxNode
{
	#region Properties
	/// <summary>The root document node in the syntax tree.</summary>
	new TDocument Document { get; }
	IFinalSyntaxNode IFinalSyntaxTree.Document => Document;
	ISyntaxNode ISyntaxTree.Document => Document;
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a complete final syntax tree (FST).
/// </summary>
/// <typeparam name="TDocument">The type of the root document node in the syntax tree.</typeparam>
public abstract class BaseFinalSyntaxTree<TDocument> : BaseSyntaxTree<TDocument>, IFinalSyntaxTree<TDocument>
	where TDocument : notnull, IFinalSyntaxNode
{
	#region Constructors
	/// <summary>Populates the <see cref="BaseFinalSyntaxTree{TDocument}"/> properties.</summary>
	/// <param name="source">The source file that the syntax tree represents.</param>
	/// <param name="document">The root document node in the syntax tree.</param>
	protected BaseFinalSyntaxTree(ISourceFile source, TDocument document) : base(source, document) { }
	#endregion
}
