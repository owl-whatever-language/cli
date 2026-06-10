namespace OwlDomain.ParsingTools.Lexing.Tokens;

/// <summary>
/// 	Represents a token node that was fabricated as an error recovery measure.
/// </summary>
public sealed class FabricatedTokenNode : BaseTokenNode
{
	#region Properties
	/// <inheritdoc/>
	public override bool IsFabricated => true;

	/// <inheritdoc/>
	public override string? Lexeme => null;
	#endregion

	#region Constructors
	/// <summary>Creates a new fabricated token.</summary>
	/// <param name="kind">The kind of the token.</param>
	/// <param name="position">The position that the token takes up.</param>
	/// <param name="leadingTrivia">The list of the leading trivia nodes.</param>
	/// <param name="trailingTrivia">The list of the trailing trivia nodes.</param>
	/// <exception cref="ArgumentException">Thrown if the given syntax <paramref name="kind"/> is not a token.</exception>
	public FabricatedTokenNode(
		SyntaxKind kind,
		IndexedPositionRange position,
		TriviaList? leadingTrivia = null,
		TriviaList? trailingTrivia = null)
		: base(kind, position, leadingTrivia, trailingTrivia)
	{
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override ITokenNode ReplaceLeadingTrivia(TriviaList newLeadingTrivia) => new FabricatedTokenNode(Kind, Position, newLeadingTrivia, TrailingTrivia);
	#endregion
}

/// <summary>
/// 	Represents a token node that was fabricated as an error recovery measure.
/// </summary>
/// <typeparam name="T">The type of value that the token can contain.</typeparam>
public sealed class FabricatedTokenNode<T> : BaseTokenNode<T>
{
	#region Properties
	/// <inheritdoc/>
	public override bool IsFabricated => true;

	/// <inheritdoc/>
	public override string? Lexeme => null;
	#endregion

	#region Constructors
	/// <summary>Creates a new fabricated token.</summary>
	/// <param name="kind">The kind of the token.</param>
	/// <param name="position">The position that the token takes up.</param>
	/// <param name="value">The value that the token contains.</param>
	/// <param name="leadingTrivia">The list of the leading trivia nodes.</param>
	/// <param name="trailingTrivia">The list of the trailing trivia nodes.</param>
	/// <exception cref="ArgumentException">Thrown if the given syntax <paramref name="kind"/> is not a token.</exception>
	public FabricatedTokenNode(
		SyntaxKind kind,
		IndexedPositionRange position,
		T value,
		TriviaList? leadingTrivia = null,
		TriviaList? trailingTrivia = null)
		: base(kind, position, value, leadingTrivia, trailingTrivia)
	{
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override ITokenNode ReplaceLeadingTrivia(TriviaList newLeadingTrivia) => new FabricatedTokenNode<T>(Kind, Position, Value, newLeadingTrivia, TrailingTrivia);
	#endregion
}
