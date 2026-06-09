namespace OwlDomain.ParsingTools.Syntax.Nodes.Semantic;

/// <summary>
/// 	Represents a complete semantic syntax tree (SST).
/// </summary>
public interface ISemanticSyntaxTree : ISyntaxTree
{
	#region Properties
	/// <summary>The abstract syntax tree (AST) that the semantic syntax tree (SST) is modelled after.</summary>
	IAbstractSyntaxTree Abstract { get; }

	/// <summary>The root node in the syntax tree.</summary>
	new ISemanticSyntaxNode Root { get; }
	ISyntaxNode ISyntaxTree.Root => Root;
	#endregion
}

/// <summary>
/// 	Represents a complete semantic syntax tree (SST).
/// </summary>
/// <typeparam name="TRoot">The type of the root syntax node.</typeparam>
public interface ISemanticSyntaxTree<out TRoot> : ISemanticSyntaxTree, ISyntaxTree<TRoot>
	where TRoot : notnull, ISemanticSyntaxNode
{
	#region Properties
	/// <summary>The root node in the syntax tree.</summary>
	new TRoot Root { get; }
	ISemanticSyntaxNode ISemanticSyntaxTree.Root => Root;
	ISyntaxNode ISyntaxTree.Root => Root;
	#endregion
}

/// <summary>
///	Represents a complete semantic syntax tree (SST).
/// </summary>
/// <typeparam name="TRoot">The type of the root syntax node.</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST) that the semantic syntax tree (SST) is modelled after.</typeparam>
public interface ISemanticSyntaxTree<out TRoot, out TAbstract> : ISemanticSyntaxTree<TRoot>
	where TRoot : notnull, ISemanticSyntaxNode
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
/// <typeparam name="TRoot">The type of the root syntax node.</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST) that the semantic syntax tree (SST) is modelled after.</typeparam>
/// <typeparam name="TAbstractRoot">The type of the root syntax node in the abstract syntax tree (AST).</typeparam>
public interface ISemanticSyntaxTree<out TRoot, out TAbstract, out TAbstractRoot> : ISemanticSyntaxTree<TRoot, TAbstract>
	where TRoot : notnull, ISemanticSyntaxNode
	where TAbstract : notnull, IAbstractSyntaxTree<TAbstractRoot>
	where TAbstractRoot : notnull, IAbstractSyntaxNode
{
}

/// <summary>
/// 	Represents the base implementation for a complete semantic syntax tree (SST).
/// </summary>
/// <typeparam name="TRoot">The type of the root syntax node.</typeparam>
/// <typeparam name="TAbstract">The type of the abstract syntax tree (AST) that the semantic syntax tree (SST) is modelled after.</typeparam>
/// <typeparam name="TAbstractRoot">The type of the root syntax node in the abstract syntax tree (AST).</typeparam>
public abstract class BaseSemanticSyntaxTree<TRoot, TAbstract, TAbstractRoot> : BaseSyntaxTree<TRoot>, ISemanticSyntaxTree<TRoot, TAbstract, TAbstractRoot>
	where TRoot : notnull, ISemanticSyntaxNode
	where TAbstract : notnull, IAbstractSyntaxTree<TAbstractRoot>
	where TAbstractRoot : notnull, IAbstractSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public TAbstract Abstract { get; }
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="BaseSemanticSyntaxTree{TRoot, TAbstract, TAbstractRoot}"/> properties.</summary>
	/// <param name="source">The source file that the syntax tree represents.</param>
	/// <param name="abstract">The abstract syntax tree that the semantic syntax tree is modelled after.</param>
	/// <param name="root">The root node in the syntax tree.</param>
	protected BaseSemanticSyntaxTree(ISourceFile source, TAbstract @abstract, TRoot root) : base(source, root)
	{
		Abstract = @abstract;
	}
	#endregion
}
