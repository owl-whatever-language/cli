namespace OwlDomain.ParsingTools.Lexing.Tokens;

/// <summary>
/// 	Represents a regular token node.
/// </summary>
public sealed class TokenNode : BaseTokenNode
{
	#region Properties
	/// <inheritdoc/>
	public override bool IsFabricated => false;

	/// <inheritdoc/>
	public override string Lexeme { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new token.</summary>
	/// <param name="kind">The kind of the token.</param>
	/// <param name="position">The position that the token takes up.</param>
	/// <param name="lexeme">The exact input that was parsed for this token.</param>
	/// <param name="leadingTrivia">The list of the leading trivia nodes.</param>
	/// <param name="trailingTrivia">The list of the trailing trivia nodes.</param>
	/// <exception cref="ArgumentException">Thrown if the given syntax <paramref name="kind"/> is not a token.</exception>
	public TokenNode(
		SyntaxKind kind,
		IndexedPositionRange position,
		string lexeme,
		TriviaList? leadingTrivia = null,
		TriviaList? trailingTrivia = null)
		: base(kind, position, leadingTrivia, trailingTrivia)
	{
		Lexeme = lexeme;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override ITokenNode ReplaceLeadingTrivia(TriviaList newLeadingTrivia) => new TokenNode(Kind, Position, Lexeme, newLeadingTrivia, TrailingTrivia);
	#endregion
}

/// <summary>
/// 	Represents a regular token node.
/// </summary>
/// <typeparam name="T">The type of value that the token can contain.</typeparam>
public sealed class TokenNode<T> : BaseTokenNode<T>
{
	#region Properties
	/// <inheritdoc/>
	public override bool IsFabricated => false;

	/// <inheritdoc/>
	public override string Lexeme { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new token.</summary>
	/// <param name="kind">The kind of the token.</param>
	/// <param name="position">The position that the token takes up.</param>
	/// <param name="lexeme">The exact input that was parsed for this token.</param>
	/// <param name="value">The value that the token contains.</param>
	/// <param name="leadingTrivia">The list of the leading trivia nodes.</param>
	/// <param name="trailingTrivia">The list of the trailing trivia nodes.</param>
	/// <exception cref="ArgumentException">Thrown if the given syntax <paramref name="kind"/> is not a token.</exception>
	public TokenNode(
		SyntaxKind kind,
		IndexedPositionRange position,
		string lexeme,
		T value,
		TriviaList? leadingTrivia = null,
		TriviaList? trailingTrivia = null)
		: base(kind, position, value, leadingTrivia, trailingTrivia)
	{
		Lexeme = lexeme;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override ITokenNode ReplaceLeadingTrivia(TriviaList newLeadingTrivia) => new TokenNode<T>(Kind, Position, Lexeme, Value, newLeadingTrivia, TrailingTrivia);
	#endregion
}
