namespace OwlDomain.ParsingTools.Semantics.Syntax;

/// <summary>
/// 	Represents a token in the semantic syntax tree (SST).
/// </summary>
public interface ISemanticSyntaxToken : ISemanticSyntaxNode, IConcreteSyntaxToken
{
	#region Properties
	/// <summary>The symbol that the token is referencing.</summary>
	ISymbol? Symbol { get; }
	#endregion
}

/// <summary>
/// 	Represents a token in the semantic syntax tree (SST).
/// </summary>
public class SemanticSyntaxToken : ConcreteSyntaxToken, ISemanticSyntaxToken
{
	#region Properties
	/// <inheritdoc/>
	public ISymbol? Symbol { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="ConcreteSyntaxToken"/> instance.</summary>
	/// <param name="kind">The kind of the token.</param>
	/// <param name="lexeme">The exact input that was parsed for this token.</param>
	/// <param name="position">The position that the token takes up.</param>
	/// <param name="leadingTrivia">The list of the leading trivia nodes.</param>
	/// <param name="trailingTrivia">The list of the trailing trivia nodes.</param>
	/// <param name="value">The value of the token.</param>
	/// <param name="isFabricated">Whether the token is fabricated.</param>
	/// <param name="classification">The classification for the token.</param>
	/// <param name="symbol">The symbol that the token is referencing.</param>
	/// <exception cref="ArgumentException">Thrown if the given syntax <paramref name="kind"/> is not a token.</exception>
	public SemanticSyntaxToken(
		SyntaxKind kind,
		string? lexeme,
		IndexedPositionRange position,
		TriviaList leadingTrivia,
		TriviaList trailingTrivia,
		object? value,
		bool isFabricated,
		ClassificationKind? classification,
		ISymbol? symbol)
		: base(kind, lexeme, position, leadingTrivia, trailingTrivia, value, isFabricated, classification)
	{
		Symbol = symbol;
	}

	/// <summary>Creates a new <see cref="ConcreteSyntaxToken"/> instance.</summary>
	/// <param name="concrete">The concrete token to inherit the values from.</param>
	/// <param name="symbol">The symbol that the token is referencing.</param>
	/// <param name="classification">The classification for the token.</param>
	public SemanticSyntaxToken(IConcreteSyntaxToken concrete, ISymbol? symbol, ClassificationKind? classification = null) : base(concrete, classification)
	{
		Symbol = symbol;
	}

	/// <summary>Copies the values from the given <paramref name="token"/>.</summary>
	/// <param name="token">The token to copy the values from.</param>
	/// <param name="classification">The classification for the token.</param>
	protected SemanticSyntaxToken(ISemanticSyntaxToken token, ClassificationKind? classification = null) : base(token, classification)
	{
		Symbol = token.Symbol;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override IEnumerable<ISemanticSyntaxNode> GetChildren() => [];
	#endregion
}
