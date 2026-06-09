namespace OwlDomain.ParsingTools.Syntax.Nodes.Concrete;

/// <summary>
/// 	Represents a complete concrete syntax tree (CST).
/// </summary>
public interface IConcreteSyntaxTree : ISyntaxTree
{
	#region Properties
	/// <summary>The root node in the syntax tree.</summary>
	new IConcreteSyntaxNode Root { get; }
	ISyntaxNode ISyntaxTree.Root => Root;
	#endregion
}

/// <summary>
/// 	Represents a complete concrete syntax tree (CST).
/// </summary>
/// <typeparam name="T">The type of the root syntax node.</typeparam>
public interface IConcreteSyntaxTree<out T> : IConcreteSyntaxTree, ISyntaxTree<T>
	where T : notnull, IConcreteSyntaxNode
{
	#region Properties
	/// <summary>The root node in the syntax tree.</summary>
	new T Root { get; }
	IConcreteSyntaxNode IConcreteSyntaxTree.Root => Root;
	ISyntaxNode ISyntaxTree.Root => Root;
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a complete concrete syntax tree (CST).
/// </summary>
/// <typeparam name="T">The type of the root syntax node.</typeparam>
public abstract class BaseConcreteSyntaxTree<T> : BaseSyntaxTree<T>, IConcreteSyntaxTree<T>
	where T : notnull, IConcreteSyntaxNode
{
	#region Constructors
	/// <summary>Populates the <see cref="BaseConcreteSyntaxTree{T}"/> properties.</summary>
	/// <param name="source">The source file that the syntax tree represents.</param>
	/// <param name="root">The root node in the syntax tree.</param>
	protected BaseConcreteSyntaxTree(ISourceFile source, T root) : base(source, root) { }
	#endregion
}
