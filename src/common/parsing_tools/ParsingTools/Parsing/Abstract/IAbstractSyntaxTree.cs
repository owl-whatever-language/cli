namespace OwlDomain.ParsingTools.Parsing.Abstract;

/// <summary>
/// 	Represents a complete abstract syntax tree (AST).
/// </summary>
public interface IAbstractSyntaxTree : ISyntaxTree
{
	#region Properties
	/// <summary>The concrete syntax tree (CST) that the abstract syntax tree (AST) is modelled after.</summary>
	IConcreteSyntaxTree Concrete { get; }

	/// <summary>The root document node in the syntax tree.</summary>
	new IAbstractSyntaxNode Document { get; }
	ISyntaxNode ISyntaxTree.Document => Document;
	#endregion
}

/// <summary>
/// 	Represents a complete abstract syntax tree (AST).
/// </summary>
/// <typeparam name="TConcrete">The type of the concrete syntax tree (CST) that the abstract syntax tree (AST) is modelled after.</typeparam>
public interface IAbstractSyntaxTree<out TConcrete> : IAbstractSyntaxTree
	where TConcrete : notnull, IConcreteSyntaxTree
{
	#region Properties
	/// <summary>The concrete syntax tree (CST) that the abstract syntax tree (AST) is modelled after.</summary>
	new TConcrete Concrete { get; }
	IConcreteSyntaxTree IAbstractSyntaxTree.Concrete => Concrete;
	#endregion
}

/// <summary>
///	Represents a complete abstract syntax tree (AST).
/// </summary>
/// <typeparam name="TConcrete">The type of the concrete syntax tree (CST) that the abstract syntax tree (AST) is modelled after.</typeparam>
/// <typeparam name="TDocument">The type of the root document node in the abstract syntax tree (AST).</typeparam>
public interface IAbstractSyntaxTree<out TConcrete, out TDocument> : IAbstractSyntaxTree<TConcrete>, ISyntaxTree<TDocument>
	where TConcrete : notnull, IConcreteSyntaxTree
	where TDocument : notnull, IAbstractSyntaxNode
{
	#region Properties
	/// <summary>The root document node in the syntax tree.</summary>
	new TDocument Document { get; }
	IAbstractSyntaxNode IAbstractSyntaxTree.Document => Document;
	ISyntaxNode ISyntaxTree.Document => Document;
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a complete abstract syntax tree (AST).
/// </summary>
/// <typeparam name="TConcrete">The type of the concrete syntax tree (CST) that the abstract syntax tree (AST) is modelled after.</typeparam>
/// <typeparam name="TDocument">The type of the root document node in the abstract syntax tree (AST).</typeparam>
public abstract class BaseAbstractSyntaxTree<TConcrete, TDocument> : BaseSyntaxTree<TDocument>, IAbstractSyntaxTree<TConcrete, TDocument>
	where TConcrete : notnull, IConcreteSyntaxTree
	where TDocument : notnull, IAbstractSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public TConcrete Concrete { get; }
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="BaseAbstractSyntaxTree{TConcrete, TDocument}"/> properties.</summary>
	/// <param name="source">The source file that the syntax tree represents.</param>
	/// <param name="concrete">The concrete syntax tree that the abstract syntax tree is modelled after.</param>
	/// <param name="document">The root document node in the syntax tree.</param>
	protected BaseAbstractSyntaxTree(ISourceFile source, TConcrete concrete, TDocument document) : base(source, document)
	{
		Concrete = concrete;
	}
	#endregion
}
