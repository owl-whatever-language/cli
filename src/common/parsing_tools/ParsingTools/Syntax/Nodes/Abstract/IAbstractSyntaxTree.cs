namespace OwlDomain.ParsingTools.Syntax.Nodes.Abstract;

/// <summary>
/// 	Represents a complete abstract syntax tree (AST).
/// </summary>
public interface IAbstractSyntaxTree : ISyntaxTree
{
	#region Properties
	/// <summary>The concrete syntax tree (CST) that the abstract syntax tree (AST) is modelled after.</summary>
	IConcreteSyntaxTree Concrete { get; }

	/// <summary>The root node in the syntax tree.</summary>
	new IAbstractSyntaxNode Root { get; }
	ISyntaxNode ISyntaxTree.Root => Root;
	#endregion
}

/// <summary>
/// 	Represents a complete abstract syntax tree (AST).
/// </summary>
/// <typeparam name="TRoot">The type of the root syntax node.</typeparam>
public interface IAbstractSyntaxTree<out TRoot> : IAbstractSyntaxTree, ISyntaxTree<TRoot>
	where TRoot : notnull, IAbstractSyntaxNode
{
	#region Properties
	/// <summary>The root node in the syntax tree.</summary>
	new TRoot Root { get; }
	IAbstractSyntaxNode IAbstractSyntaxTree.Root => Root;
	ISyntaxNode ISyntaxTree.Root => Root;
	#endregion
}

/// <summary>
///	Represents a complete abstract syntax tree (AST).
/// </summary>
/// <typeparam name="TRoot">The type of the root syntax node.</typeparam>
/// <typeparam name="TConcrete">The type of the concrete syntax tree (CST) that the abstract syntax tree (AST) is modelled after.</typeparam>
public interface IAbstractSyntaxTree<out TRoot, out TConcrete> : IAbstractSyntaxTree<TRoot>
	where TRoot : notnull, IAbstractSyntaxNode
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
/// <typeparam name="TRoot">The type of the root syntax node.</typeparam>
/// <typeparam name="TConcrete">The type of the concrete syntax tree (CST) that the abstract syntax tree (AST) is modelled after.</typeparam>
/// <typeparam name="TConcreteRoot">The type of the root syntax node in the concrete syntax tree (CST).</typeparam>
public interface IAbstractSyntaxTree<out TRoot, out TConcrete, out TConcreteRoot> : IAbstractSyntaxTree<TRoot, TConcrete>
	where TRoot : notnull, IAbstractSyntaxNode
	where TConcrete : notnull, IConcreteSyntaxTree<TConcreteRoot>
	where TConcreteRoot : notnull, IConcreteSyntaxNode
{
}

/// <summary>
/// 	Represents the base implementation for a complete abstract syntax tree (AST).
/// </summary>
/// <typeparam name="TRoot">The type of the root syntax node.</typeparam>
/// <typeparam name="TConcrete">The type of the concrete syntax tree (CST) that the abstract syntax tree (AST) is modelled after.</typeparam>
/// <typeparam name="TConcreteRoot">The type of the root syntax node in the concrete syntax tree (CST).</typeparam>
public abstract class BaseAbstractSyntaxTree<TRoot, TConcrete, TConcreteRoot> : BaseSyntaxTree<TRoot>, IAbstractSyntaxTree<TRoot, TConcrete, TConcreteRoot>
	where TRoot : notnull, IAbstractSyntaxNode
	where TConcrete : notnull, IConcreteSyntaxTree<TConcreteRoot>
	where TConcreteRoot : notnull, IConcreteSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public TConcrete Concrete { get; }
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="BaseAbstractSyntaxTree{TRoot, TConcrete, TConcreteRoot}"/> properties.</summary>
	/// <param name="source">The source file that the syntax tree represents.</param>
	/// <param name="concrete">The concrete syntax tree that the abstract syntax tree is modelled after.</param>
	/// <param name="root">The root node in the syntax tree.</param>
	protected BaseAbstractSyntaxTree(ISourceFile source, TConcrete concrete, TRoot root) : base(source, root)
	{
		Concrete = concrete;
	}
	#endregion
}
