namespace OwlDomain.ParsingTools.Syntax.Nodes.Abstract;

/// <summary>
/// 	Represents a syntax node in the abstract syntax tree (AST).
/// </summary>
public interface IAbstractSyntaxNode : ISyntaxNode
{
	#region Properties
	/// <summary>The concrete syntax node that this abstract syntax node was created from.</summary>
	IConcreteSyntaxNode Concrete { get; }
	#endregion
}
