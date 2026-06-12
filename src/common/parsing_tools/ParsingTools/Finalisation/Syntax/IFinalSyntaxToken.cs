namespace OwlDomain.ParsingTools.Finalisation.Syntax;

/// <summary>
/// 	Represents a token in the final syntax tree (FST).
/// </summary>
public interface IFinalSyntaxToken : IFinalSyntaxNode, ISemanticSyntaxToken
{
}

/// <summary>
/// 	Represents a token in the final syntax tree (FST).
/// </summary>
public class FinalSyntaxToken : SemanticSyntaxToken, IFinalSyntaxToken
{
	#region Constructors
	/// <summary>Creates a new <see cref="SemanticSyntaxToken"/> instance.</summary>
	/// <param name="kind">The kind of the token.</param>
	/// <param name="lexeme">The exact input that was parsed for this token.</param>
	/// <param name="position">The position that the token takes up.</param>
	/// <param name="leadingTrivia">The list of the leading trivia nodes.</param>
	/// <param name="trailingTrivia">The list of the trailing trivia nodes.</param>
	/// <param name="value">The value of the token.</param>
	/// <param name="isFabricated">Whether the token is fabricated.</param>
	/// <param name="classification">The classifications for the token.</param>
	/// <param name="symbol">The symbol that the token is referencing.</param>
	/// <exception cref="ArgumentException">Thrown if the given syntax <paramref name="kind"/> is not a token.</exception>
	public FinalSyntaxToken(
		SyntaxKind kind,
		string? lexeme,
		IndexedPositionRange position,
		TriviaList leadingTrivia,
		TriviaList trailingTrivia,
		object? value,
		bool isFabricated,
		ClassificationList classification,
		ISymbol? symbol)
		: base(kind, lexeme, position, leadingTrivia, trailingTrivia, value, isFabricated, classification, symbol)
	{
	}

	/// <summary>Creates a new <see cref="SemanticSyntaxToken"/> instance.</summary>
	/// <param name="semantic">The semantic token to inherit the values from.</param>
	public FinalSyntaxToken(SemanticSyntaxToken semantic) : base(semantic)
	{
	}

	/// <summary>Copies the values from the given <paramref name="token"/>.</summary>
	/// <param name="token">The token to copy the values from.</param>
	protected FinalSyntaxToken(FinalSyntaxToken token) : base(token)
	{
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override IEnumerable<IFinalSyntaxNode> GetChildren() => [];
	#endregion
}
