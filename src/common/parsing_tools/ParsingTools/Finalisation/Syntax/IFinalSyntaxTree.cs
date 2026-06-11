namespace OwlDomain.ParsingTools.Finalisation.Syntax;

/// <summary>
/// 	Represents a complete final syntax tree (FST).
/// </summary>
public interface IFinalSyntaxTree : ISyntaxTree
{
	#region Properties
	/// <summary>The semantic syntax tree (SST) that the final syntax tree (FST) is modelled after.</summary>
	ISemanticSyntaxTree Semantic { get; }

	/// <summary>The root document node in the syntax tree.</summary>
	new IFinalSyntaxNode Document { get; }
	ISyntaxNode ISyntaxTree.Document => Document;
	#endregion
}

/// <summary>
/// 	Represents a complete final syntax tree (FST).
/// </summary>
/// <typeparam name="TSemantic">The type of the semantic syntax tree (SST) that the final syntax tree (FST) is modelled after.</typeparam>
public interface IFinalSyntaxTree<out TSemantic> : IFinalSyntaxTree
	where TSemantic : notnull, ISemanticSyntaxTree
{
	#region Properties
	/// <summary>The semantic syntax tree (SST) that the final syntax tree (FST) is modelled after.</summary>
	new TSemantic Semantic { get; }
	ISemanticSyntaxTree IFinalSyntaxTree.Semantic => Semantic;
	#endregion
}

/// <summary>
///	Represents a complete final syntax tree (FST).
/// </summary>
/// <typeparam name="TSemantic">The type of the semantic syntax tree (SST) that the final syntax tree (FST) is modelled after.</typeparam>
/// <typeparam name="TDocument">The type of the root document node in the final syntax tree (FST).</typeparam>
public interface IFinalSyntaxTree<out TSemantic, out TDocument> : IFinalSyntaxTree<TSemantic>, ISyntaxTree<TDocument>
	where TSemantic : notnull, ISemanticSyntaxTree
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
/// <typeparam name="TSemantic">The type of the semantic syntax tree (SST) that the final syntax tree (FST) is modelled after.</typeparam>
/// <typeparam name="TDocument">The type of the root document node in the final syntax tree (FST).</typeparam>
public abstract class BaseFinalSyntaxTree<TSemantic, TDocument> : BaseSyntaxTree<TDocument>, IFinalSyntaxTree<TSemantic, TDocument>
	where TSemantic : notnull, ISemanticSyntaxTree
	where TDocument : notnull, IFinalSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public TSemantic Semantic { get; }
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="BaseFinalSyntaxTree{TSemantic, TDocument}"/> properties.</summary>
	/// <param name="source">The source file that the syntax tree represents.</param>
	/// <param name="semantic">The semantic syntax tree that the final syntax tree is modelled after.</param>
	/// <param name="document">The root document node in the syntax tree.</param>
	protected BaseFinalSyntaxTree(ISourceFile source, TSemantic semantic, TDocument document) : base(source, document)
	{
		Semantic = semantic;
	}
	#endregion
}

/// <summary>
/// 	Contains various extensions related to the final syntax tree (FST).
/// </summary>
public static class IFinalSyntaxTreeExtensions
{
	extension<TAbstract>(IFinalSyntaxTree<ISemanticSyntaxTree<TAbstract>> tree)
			where TAbstract : notnull, IAbstractSyntaxTree
	{
		#region Properties
		/// <summary>The abstract syntax tree (AST) that the final syntax tree (FST) is modelled after.</summary>
		public TAbstract Abstract => tree.Semantic.Abstract;
		#endregion
	}

	extension<TConcrete>(IFinalSyntaxTree<ISemanticSyntaxTree<IAbstractSyntaxTree<TConcrete>>> tree)
		where TConcrete : notnull, IConcreteSyntaxTree
	{
		#region Properties
		/// <summary>The concrete syntax tree (CST) that the final syntax tree (FST) is modelled after.</summary>
		public TConcrete Concrete => tree.Semantic.Abstract.Concrete;
		#endregion
	}
}
