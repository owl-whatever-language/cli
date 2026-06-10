namespace OwlDomain.ParsingTools.Semantics;

/// <summary>
/// 	Represents a syntax node in the semantic syntax tree (SST).
/// </summary>
public interface ISemanticSyntaxNode : ISyntaxNode
{
	#region Properties
	/// <summary>The abstract syntax node that this semantic syntax node is modelled after.</summary>
	IAbstractSyntaxNode Abstract { get; }
	#endregion
}

/// <summary>
/// 	Represents a syntax node in the semantic syntax tree (SST).
/// </summary>
/// <typeparam name="T">The type of the abstract syntax node that the semantic syntax node is modelled after.</typeparam>
public interface ISemanticSyntaxNode<out T> : ISemanticSyntaxNode
	where T : notnull, IAbstractSyntaxNode
{
	#region Properties
	/// <summary>The abstract syntax node that this semantic syntax node is modelled after.</summary>
	new T Abstract { get; }
	IAbstractSyntaxNode ISemanticSyntaxNode.Abstract => Abstract;
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a syntax node in the semantic syntax tree (SST).
/// </summary>
/// <typeparam name="T">The type of the abstract syntax node that the semantic syntax node is modelled after.</typeparam>
public abstract class BaseSemanticSyntaxNode<T> : ISemanticSyntaxNode<T>
	where T : notnull, IAbstractSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public T Abstract { get; }

	/// <inheritdoc/>
	public virtual SyntaxKind Kind => Abstract.Kind;

	/// <inheritdoc/>
	public IndexedPositionRange Position => Abstract.Position;
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="BaseSemanticSyntaxNode{T}"/> properties.</summary>
	/// <param name="abstract">The abstract syntax tree that this semantic syntax node is modelled after.</param>
	protected BaseSemanticSyntaxNode(T @abstract)
	{
		Abstract = @abstract;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public abstract IEnumerable<ISyntaxNode> GetChildren();

	/// <inheritdoc/>
	public override string? ToString() => Abstract.ToString();
	#endregion
}

/// <summary>
/// 	Contains various extensions related to the semantic syntax nodes.
/// </summary>
public static class ISemanticSyntaxNodeExtensions
{
	extension<TConcrete>(ISemanticSyntaxNode<IAbstractSyntaxNode<TConcrete>> node)
		where TConcrete : notnull, IConcreteSyntaxNode
	{
		#region Properties
		/// <summary>The concrete syntax node that the semantic syntax node is modelled after.</summary>
		public TConcrete Concrete => node.Abstract.Concrete;
		#endregion
	}
}
