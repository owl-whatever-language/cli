namespace OwlDomain.ParsingTools.Finalisation.Syntax;

/// <summary>
/// 	Represents a syntax node in the final syntax tree (FST).
/// </summary>
public interface IFinalSyntaxNode : ISyntaxNode
{
	#region Properties
	/// <summary>The semantic syntax node that this final syntax node is modelled after.</summary>
	ISemanticSyntaxNode Semantic { get; }
	#endregion

	#region Methods
	/// <summary>Gets the direct child syntax nodes.</summary>
	/// <returns>An enumerable of the direct children.</returns>
	new IEnumerable<IFinalSyntaxNode> GetChildren();
	IEnumerable<ISyntaxNode> ISyntaxNode.GetChildren() => GetChildren();
	#endregion
}

/// <summary>
/// 	Represents a syntax node in the final syntax tree (FST).
/// </summary>
/// <typeparam name="TSemantic">The type of the semantic syntax node that the final syntax node is modelled after.</typeparam>
public interface IFinalSyntaxNode<out TSemantic> : IFinalSyntaxNode
	where TSemantic : notnull, ISemanticSyntaxNode
{
	#region Properties
	/// <summary>The semantic syntax node that this final syntax node is modelled after.</summary>
	new TSemantic Semantic { get; }
	ISemanticSyntaxNode IFinalSyntaxNode.Semantic => Semantic;
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a syntax node in the final syntax tree (FST).
/// </summary>
/// <typeparam name="TSemantic">The type of the semantic syntax node that the final syntax node is modelled after.</typeparam>
public abstract class BaseFinalSyntaxNode<TSemantic> : IFinalSyntaxNode<TSemantic>
	where TSemantic : notnull, ISemanticSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public TSemantic Semantic { get; }

	/// <inheritdoc/>
	public virtual SyntaxKind Kind => Semantic.Kind;

	/// <inheritdoc/>
	public IndexedPositionRange Position => Semantic.Position;
	#endregion

	#region Constructors
	/// <summary>Populates the <see cref="BaseFinalSyntaxNode{T}"/> properties.</summary>
	/// <param name="semantic">The semantic syntax tree that this final syntax node is modelled after.</param>
	protected BaseFinalSyntaxNode(TSemantic semantic)
	{
		Semantic = semantic;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public abstract IEnumerable<IFinalSyntaxNode> GetChildren();

	/// <inheritdoc/>
	public override string? ToString() => Semantic.ToString();
	#endregion
}

/// <summary>
/// 	Contains various extensions related to the final syntax nodes.
/// </summary>
public static class IFinalSyntaxNodeExtensions
{
	extension<TAbstract>(IFinalSyntaxNode<ISemanticSyntaxNode<TAbstract>> node)
			where TAbstract : notnull, IAbstractSyntaxNode
	{
		#region Properties
		/// <summary>The abstract syntax node that the final syntax node is modelled after.</summary>
		public TAbstract Abstract => node.Semantic.Abstract;
		#endregion
	}

	extension<TConcrete>(IFinalSyntaxNode<ISemanticSyntaxNode<IAbstractSyntaxNode<TConcrete>>> node)
		where TConcrete : notnull, IConcreteSyntaxNode
	{
		#region Properties
		/// <summary>The concrete syntax node that the final syntax node is modelled after.</summary>
		public TConcrete Concrete => node.Semantic.Abstract.Concrete;
		#endregion
	}
}
