namespace OwlDomain.ParsingTools.Semantics;

/// <summary>
/// 	Represents a complete semantic syntax tree (SST).
/// </summary>
public interface ISemanticSyntaxTree : ISyntaxTree
{
	#region Properties
	/// <summary>The abstract syntax tree (AST) that the semantic syntax tree (SST) is modelled after.</summary>
	IAbstractSyntaxTree Abstract { get; }

	/// <summary>The root document node in the syntax tree.</summary>
	new ISemanticSyntaxNode Document { get; }
	ISyntaxNode ISyntaxTree.Document => Document;
	#endregion
}

/// <summary>
/// 	Represents a complete semantic syntax tree (SST).
/// </summary>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST) that the semantic syntax tree (SST) is modelled after.</typeparam>
public interface ISemanticSyntaxTree<out TAbstract> : ISemanticSyntaxTree
	where TAbstract : notnull, IAbstractSyntaxTree
{
	#region Properties
	/// <summary>The abstract syntax tree (AST) that the semantic syntax tree (SST) is modelled after.</summary>
	new TAbstract Abstract { get; }
	IAbstractSyntaxTree ISemanticSyntaxTree.Abstract => Abstract;
	#endregion

}

/// <summary>
///	Represents a complete semantic syntax tree (SST).
/// </summary>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST) that the semantic syntax tree (SST) is modelled after.</typeparam>
/// <typeparam name="TDocument">The type of the root document node in the syntax tree.</typeparam>
public interface ISemanticSyntaxTree<out TAbstract, out TDocument> : ISemanticSyntaxTree<TAbstract>, ISyntaxTree<TDocument>
	where TAbstract : notnull, IAbstractSyntaxTree
	where TDocument : notnull, ISemanticSyntaxNode
{
	#region Properties
	/// <summary>The root node in the syntax tree.</summary>
	new TDocument Document { get; }
	ISemanticSyntaxNode ISemanticSyntaxTree.Document => Document;
	ISyntaxNode ISyntaxTree.Document => Document;
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a complete semantic syntax tree (SST).
/// </summary>
/// <typeparam name="TDocument">The type of the root document node in the syntax tree.</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST) that the semantic syntax tree (SST) is modelled after.</typeparam>
public abstract class BaseSemanticSyntaxTree<TAbstract, TDocument> : BaseSyntaxTree<TDocument>, ISemanticSyntaxTree<TAbstract, TDocument>
	where TDocument : notnull, ISemanticSyntaxNode
	where TAbstract : notnull, IAbstractSyntaxTree
{
	#region Properties
	/// <inheritdoc/>
	public TAbstract Abstract { get; }
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="BaseSemanticSyntaxTree{TAbstract, TDocument}"/> properties.</summary>
	/// <param name="source">The source file that the syntax tree represents.</param>
	/// <param name="abstract">The abstract syntax tree that the semantic syntax tree is modelled after.</param>
	/// <param name="document">The root node in the syntax tree.</param>
	protected BaseSemanticSyntaxTree(ISourceFile source, TAbstract @abstract, TDocument document) : base(source, document)
	{
		Abstract = @abstract;
	}
	#endregion
}
