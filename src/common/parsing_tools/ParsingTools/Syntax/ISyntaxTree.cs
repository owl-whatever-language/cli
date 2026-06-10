namespace OwlDomain.ParsingTools.Syntax;

/// <summary>
/// 	Represents a complete syntax tree.
/// </summary>
public interface ISyntaxTree
{
	#region Properties
	/// <summary>The source file that the syntax tree represents.</summary>
	ISourceFile Source { get; }

	/// <summary>The root node in the syntax tree.</summary>
	ISyntaxNode Root { get; }
	#endregion
}

/// <summary>
/// 	Represents a complete syntax tree.
/// </summary>
/// <typeparam name="T">The type of the root syntax node.</typeparam>
public interface ISyntaxTree<out T> : ISyntaxTree
	where T : notnull, ISyntaxNode
{
	#region Properties
	/// <summary>The root node in the syntax tree.</summary>
	new T Root { get; }
	ISyntaxNode ISyntaxTree.Root => Root;
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a complete syntax tree.
/// </summary>
/// <typeparam name="T">The type of the root syntax node.</typeparam>
public abstract class BaseSyntaxTree<T> : ISyntaxTree<T>
	where T : notnull, ISyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public ISourceFile Source { get; }

	/// <inheritdoc/>
	public T Root { get; }
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="BaseSyntaxTree{T}"/> properties.</summary>
	/// <param name="source">The source file that the syntax tree represents.</param>
	/// <param name="root">The root node in the syntax tree.</param>
	protected BaseSyntaxTree(ISourceFile source, T root)
	{
		Source = source;
		Root = root;
	}
	#endregion
}
