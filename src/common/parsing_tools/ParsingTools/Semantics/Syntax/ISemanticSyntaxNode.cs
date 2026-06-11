namespace OwlDomain.ParsingTools.Semantics.Syntax;

/// <summary>
/// 	Represents a syntax node in the semantic syntax tree (SST).
/// </summary>
public interface ISemanticSyntaxNode : ISyntaxNode
{
	#region Properties
	/// <summary>The abstract syntax node that this semantic syntax node is modelled after.</summary>
	IAbstractSyntaxNode Abstract { get; }
	#endregion

	#region Methods
	/// <summary>Gets the direct child syntax nodes.</summary>
	/// <returns>An enumerable of the direct children.</returns>
	new IEnumerable<ISemanticSyntaxNode> GetChildren();
	IEnumerable<ISyntaxNode> ISyntaxNode.GetChildren() => GetChildren();
	#endregion
}

/// <summary>
/// 	Represents a syntax node in the semantic syntax tree (SST).
/// </summary>
/// <typeparam name="TAbstract">The type of the abstract syntax node that the semantic syntax node is modelled after.</typeparam>
public interface ISemanticSyntaxNode<out TAbstract> : ISemanticSyntaxNode
	where TAbstract : notnull, IAbstractSyntaxNode
{
	#region Properties
	/// <summary>The abstract syntax node that this semantic syntax node is modelled after.</summary>
	new TAbstract Abstract { get; }
	IAbstractSyntaxNode ISemanticSyntaxNode.Abstract => Abstract;
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a syntax node in the semantic syntax tree (SST).
/// </summary>
/// <typeparam name="TAbstract">The type of the abstract syntax node that the semantic syntax node is modelled after.</typeparam>
public abstract class BaseSemanticSyntaxNode<TAbstract> : ISemanticSyntaxNode<TAbstract>
	where TAbstract : notnull, IAbstractSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public TAbstract Abstract { get; }

	/// <inheritdoc/>
	public virtual SyntaxKind Kind => Abstract.Kind;

	/// <inheritdoc/>
	public IndexedPositionRange Position => Abstract.Position;
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="BaseSemanticSyntaxNode{T}"/> properties.</summary>
	/// <param name="abstract">The abstract syntax tree that this semantic syntax node is modelled after.</param>
	protected BaseSemanticSyntaxNode(TAbstract @abstract)
	{
		Abstract = @abstract;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public abstract IEnumerable<ISemanticSyntaxNode> GetChildren();

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
