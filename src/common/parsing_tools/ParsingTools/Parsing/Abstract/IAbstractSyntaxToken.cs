namespace OwlDomain.ParsingTools.Parsing.Abstract;

/// <summary>
/// 	Represents a token in the abstract syntax tree (AST).
/// </summary>
public interface IAbstractSyntaxToken : IAbstractSyntaxNode<ITokenNode>
{
	#region Properties
	/// <summary>The value of the token, if it has one.</summary>
	object? Value { get; }
	#endregion
}

/// <summary>
/// 	Represents a token in the abstract syntax tree (AST).
/// </summary>
public class AbstractSyntaxToken : BaseAbstractSyntaxNode<ITokenNode>, IAbstractSyntaxToken
{
	#region Properties
	/// <inheritdoc/>
	public object? Value => Concrete.Value;
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="AbstractSyntaxToken"/> instance.</summary>
	/// <param name="concrete">The token in the concrete syntax tree (CST).</param>
	public AbstractSyntaxToken(ITokenNode concrete) : base(concrete) { }
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override IEnumerable<IAbstractSyntaxNode> GetChildren() => [];
	#endregion
}
