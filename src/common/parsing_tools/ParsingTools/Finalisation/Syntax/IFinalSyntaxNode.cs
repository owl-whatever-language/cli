namespace OwlDomain.ParsingTools.Finalisation.Syntax;

/// <summary>
/// 	Represents a syntax node in the final syntax tree (FST).
/// </summary>
public interface IFinalSyntaxNode : ISyntaxNode<IFinalSyntaxNode>, ISemanticSyntaxNode
{
	#region Methods
	/// <summary>Gets the direct child syntax nodes.</summary>
	/// <returns>An enumerable of the direct children.</returns>
	new IEnumerable<IFinalSyntaxNode> GetChildren();
	IEnumerable<ISyntaxNode> ISyntaxNode.GetChildren() => GetChildren();
	IEnumerable<IConcreteSyntaxNode> ISyntaxNode<IConcreteSyntaxNode>.GetChildren() => GetChildren();
	IEnumerable<ISemanticSyntaxNode> ISyntaxNode<ISemanticSyntaxNode>.GetChildren() => GetChildren();
	IEnumerable<ISemanticSyntaxNode> ISemanticSyntaxNode.GetChildren() => GetChildren();
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a syntax node in the final syntax tree (FST).
/// </summary>
public abstract class BaseFinalSyntaxNode : BaseSyntaxNode<IFinalSyntaxNode>, IFinalSyntaxNode
{
}

/// <summary>
/// 	Contains various extensions related to the <see cref="IFinalSyntaxNode"/>.
/// </summary>
public static class IFinalSyntaxNodeExtensions
{
	extension(IFinalSyntaxNode node)
	{
		#region Methods
		/// <summary>Flattens the current node to all of the leaf tokens that make it up.</summary>
		/// <returns>A list of the leaf tokens that make up the current node.</returns>
		public IReadOnlyList<IFinalSyntaxToken> Flatten() => node.Flatten<IFinalSyntaxToken>();
		#endregion
	}
}