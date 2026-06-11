namespace OwlDomain.ParsingTools.Semantics.Syntax;

/// <summary>
/// 	Represents a token in the semantic syntax tree (SST).
/// </summary>
public interface ISemanticSyntaxToken : ISemanticSyntaxNode<IAbstractSyntaxToken>
{
	#region Properties
	/// <summary>The value of the token, if it has one.</summary>
	object? Value { get; }

	/// <summary>The symbol that the token is referencing.</summary>
	ISymbol? Symbol { get; }
	#endregion
}

/// <summary>
/// 	Represents a token in the semantic syntax tree (SST).
/// </summary>
public class SemanticSyntaxToken : BaseSemanticSyntaxNode<IAbstractSyntaxToken>, ISemanticSyntaxToken
{
	#region Properties
	/// <inheritdoc/>
	public object? Value => Abstract.Value;

	/// <inheritdoc/>
	public ISymbol? Symbol { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="SemanticSyntaxToken"/> instance.</summary>
	/// <param name="abstract">The token in the abstract syntax tree (AST).</param>
	/// <param name="symbol">The symbol that the token is referencing.</param>
	public SemanticSyntaxToken(IAbstractSyntaxToken @abstract, ISymbol? symbol = null) : base(@abstract)
	{
		Symbol = symbol;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override IEnumerable<ISemanticSyntaxNode> GetChildren() => [];
	#endregion
}
