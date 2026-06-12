namespace OwlDomain.ParsingTools.Semantics.Syntax;

/// <summary>
/// 	Represents a syntax node in the semantic syntax tree (SST).
/// </summary>
public interface ISemanticSyntaxNode : ISyntaxNode<ISemanticSyntaxNode>, IConcreteSyntaxNode
{
	#region Methods
	/// <summary>Gets the direct child syntax nodes.</summary>
	/// <returns>An enumerable of the direct children.</returns>
	new IEnumerable<ISemanticSyntaxNode> GetChildren();
	IEnumerable<ISyntaxNode> ISyntaxNode.GetChildren() => GetChildren();
	IEnumerable<IConcreteSyntaxNode> ISyntaxNode<IConcreteSyntaxNode>.GetChildren() => GetChildren();
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a syntax node in the semantic syntax tree (SST).
/// </summary>
public abstract class BaseSemanticSyntaxNode : BaseSyntaxNode<ISemanticSyntaxNode>, ISemanticSyntaxNode
{
}
