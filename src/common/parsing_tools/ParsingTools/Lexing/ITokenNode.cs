namespace OwlDomain.ParsingTools.Lexing;

/// <summary>
/// 	Represents a token node produced by the lexer.
/// </summary>
public interface ITokenNode : ISyntaxNode
{
	#region Properties
	/// <summary>The list of the leading trivia nodes.</summary>
	TriviaList LeadingTrivia { get; }

	/// <summary>The list of the trailing trivia nodes.</summary>
	TriviaList TrailingTrivia { get; }

	/// <summary>The exact input that was parsed for this token.</summary>
	string Lexeme { get; }

	/// <summary>The value of the token.</summary>
	object? Value { get; }
	#endregion

	#region Methods
	/// <summary>Creates a new token with the same properties, but with the <paramref name="newLeadingTrivia"/> list instead.</summary>
	/// <param name="newLeadingTrivia">The new leading trivia list.</param>
	/// <returns>A duplicate of the current token with the <paramref name="newLeadingTrivia"/> list.</returns>
	ITokenNode ReplaceLeadingTrivia(TriviaList newLeadingTrivia);
	#endregion
}

/// <summary>
/// 	Represents a token node produced by the lexer.
/// </summary>
public class TokenNode : ITokenNode
{
	#region Properties
	/// <inheritdoc/>
	public SyntaxKind Kind { get; }

	/// <inheritdoc/>
	public TriviaList LeadingTrivia { get; }

	/// <inheritdoc/>
	public TriviaList TrailingTrivia { get; }

	/// <inheritdoc/>
	public string Lexeme { get; }

	/// <inheritdoc/>
	public IndexedPositionRange FullPosition
	{
		get
		{
			IndexedLinePosition start = LeadingTrivia.Count > 0 ? LeadingTrivia[0].Position.Start : Position.Start;
			IndexedLinePosition end = TrailingTrivia.Count > 0 ? TrailingTrivia[^1].Position.End : Position.End;

			return new(start, end);
		}
	}

	/// <inheritdoc/>
	public IndexedPositionRange Position { get; }

	/// <inheritdoc/>
	public object? Value { get; }
	#endregion

	#region Constructors
	/// <summary>Populates the properties on the base token node.</summary>
	/// <param name="kind">The kind of the token.</param>
	/// <param name="lexeme">The exact input that was parsed for this token.</param>
	/// <param name="position">The position that the token takes up.</param>
	/// <param name="leadingTrivia">The list of the leading trivia nodes.</param>
	/// <param name="trailingTrivia">The list of the trailing trivia nodes.</param>
	/// <param name="value">The value of the token.</param>
	/// <exception cref="ArgumentException">Thrown if the given syntax <paramref name="kind"/> is not a token.</exception>
	public TokenNode(
		SyntaxKind kind,
		string lexeme,
		IndexedPositionRange position,
		TriviaList? leadingTrivia,
		TriviaList? trailingTrivia,
		object? value)
	{
		Guard.IsOfCategory(kind, SyntaxCategory.Token);

		if (value is not null)
			Guard.IsNotOfType<TriviaList>(value);

		Kind = kind;
		Lexeme = lexeme;
		Position = position;
		Value = value;

		LeadingTrivia = leadingTrivia ?? [];
		TrailingTrivia = trailingTrivia ?? [];
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public ITokenNode ReplaceLeadingTrivia(TriviaList newLeadingTrivia) => new TokenNode(Kind, Lexeme, Position, newLeadingTrivia, TrailingTrivia, Value);

	/// <inheritdoc/>
	public IEnumerable<ISyntaxNode> GetChildren() => LeadingTrivia.Concat(TrailingTrivia);

	/// <inheritdoc/>
	public override string ToString() => Lexeme;
	#endregion
}
