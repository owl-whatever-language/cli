namespace OwlDomain.ParsingTools.Parsing.Abstract;

/// <summary>
/// 	Represents a syntax node in the abstract syntax tree (AST).
/// </summary>
public interface IAbstractSyntaxNode : ISyntaxNode
{
	#region Properties
	/// <summary>The concrete syntax node that this abstract syntax node is modelled after.</summary>
	IConcreteSyntaxNode Concrete { get; }
	#endregion
}

/// <summary>
/// 	Represents a syntax node in the abstract syntax tree (AST).
/// </summary>
/// <typeparam name="TConcrete">The type of the concrete syntax node that the abstract syntax node is modelled after.</typeparam>
public interface IAbstractSyntaxNode<out TConcrete> : IAbstractSyntaxNode
	where TConcrete : notnull, IConcreteSyntaxNode
{
	#region Properties
	/// <summary>The concrete syntax node that this abstract syntax node is modelled after.</summary>
	new TConcrete Concrete { get; }
	IConcreteSyntaxNode IAbstractSyntaxNode.Concrete => Concrete;
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a syntax node in the abstract syntax tree (AST).
/// </summary>
/// <typeparam name="TConcrete">The type of the concrete syntax node that the abstract syntax node is modelled after.</typeparam>
public abstract class BaseAbstractSyntaxNode<TConcrete> : IAbstractSyntaxNode<TConcrete>
	where TConcrete : notnull, IConcreteSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public TConcrete Concrete { get; }

	/// <inheritdoc/>
	public virtual SyntaxKind Kind => Concrete.Kind;

	/// <inheritdoc/>
	public IndexedPositionRange Position => Concrete.Position;
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="BaseAbstractSyntaxNode{T}"/> properties.</summary>
	/// <param name="concrete">The concrete syntax tree that this abstract syntax node is modelled after.</param>
	protected BaseAbstractSyntaxNode(TConcrete concrete)
	{
		Concrete = concrete;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public abstract IEnumerable<ISyntaxNode> GetChildren();

	/// <inheritdoc/>
	public override string? ToString() => Concrete.ToString();
	#endregion
}
