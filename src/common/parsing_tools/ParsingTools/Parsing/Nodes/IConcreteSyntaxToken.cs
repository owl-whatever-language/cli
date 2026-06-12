namespace OwlDomain.ParsingTools.Parsing.Nodes;

/// <summary>
/// 	Represents a token node in the concrete syntax tree (CST).
/// </summary>
public interface IConcreteSyntaxToken : IConcreteSyntaxNode
{
	#region Properties
	/// <summary>The list of the leading trivia nodes.</summary>
	TriviaList LeadingTrivia { get; }

	/// <summary>The list of the trailing trivia nodes.</summary>
	TriviaList TrailingTrivia { get; }

	/// <summary>The exact input that was parsed for this token.</summary>
	string? Lexeme { get; }

	/// <summary>The value of the token.</summary>
	object? Value { get; }

	/// <summary>The classifications of the token.</summary>
	ClassificationList Classification { get; }
	#endregion
}

/// <summary>
/// 	Represents a token node in the concrete syntax tree (CST).
/// </summary>
public class ConcreteSyntaxToken : BaseConcreteSyntaxNode, IConcreteSyntaxToken
{
	#region Properties
	/// <inheritdoc/>
	public override SyntaxKind Kind { get; }

	/// <inheritdoc/>
	public string? Lexeme { get; }

	/// <inheritdoc/>
	public override IndexedPositionRange Position { get; }

	/// <inheritdoc/>
	public override IndexedPositionRange FullPosition
	{
		get
		{
			IndexedLinePosition start = LeadingTrivia.Count > 0 ? LeadingTrivia[0].Position.Start : Position.Start;
			IndexedLinePosition end = TrailingTrivia.Count > 0 ? TrailingTrivia[^1].Position.End : Position.End;

			return new(start, end);
		}
	}

	/// <inheritdoc/>
	public TriviaList LeadingTrivia { get; }

	/// <inheritdoc/>
	public TriviaList TrailingTrivia { get; }

	/// <inheritdoc/>
	public object? Value { get; }

	/// <inheritdoc/>
	public override bool IsFabricated { get; }

	/// <inheritdoc/>
	public ClassificationList Classification { get; }
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
	/// <param name="classification">The classifications for the token.</param>
	/// <exception cref="ArgumentException">Thrown if the given syntax <paramref name="kind"/> is not a token.</exception>
	protected ConcreteSyntaxToken(
		SyntaxKind kind,
		string? lexeme,
		IndexedPositionRange position,
		TriviaList leadingTrivia,
		TriviaList trailingTrivia,
		object? value,
		bool isFabricated,
		ClassificationList classification)
	{
		Guard.IsOfCategory(kind, SyntaxCategory.Token);

		Kind = kind;
		Lexeme = lexeme;
		Position = position;
		LeadingTrivia = leadingTrivia;
		TrailingTrivia = trailingTrivia;
		Value = value;
		IsFabricated = isFabricated;
		Classification = classification;
	}

	/// <summary>Copies the values from the given <paramref name="token"/>.</summary>
	/// <param name="token">The token to copy the values from.</param>
	/// <param name="classification">The new classifications for the token.</param>
	protected ConcreteSyntaxToken(IConcreteSyntaxToken token, ClassificationList? classification = null)
	{
		Kind = token.Kind;
		Lexeme = token.Lexeme;
		Position = token.Position;
		LeadingTrivia = token.LeadingTrivia;
		TrailingTrivia = token.TrailingTrivia;
		Value = token.Value;
		IsFabricated = token.IsFabricated;
		Classification = classification ?? token.Classification;
	}

	/// <summary>Creates a new <see cref="ConcreteSyntaxToken"/> instance.</summary>
	/// <param name="kind">The kind of the token.</param>
	/// <param name="lexeme">The exact input that was parsed for this token.</param>
	/// <param name="position">The position that the token takes up.</param>
	/// <param name="leadingTrivia">The list of the leading trivia nodes.</param>
	/// <param name="trailingTrivia">The list of the trailing trivia nodes.</param>
	/// <param name="value">The value of the token.</param>
	/// <param name="classification">The classifications for the token.</param>
	/// <exception cref="ArgumentException">Thrown if the given syntax <paramref name="kind"/> is not a token.</exception>
	public ConcreteSyntaxToken(
		SyntaxKind kind,
		string lexeme,
		IndexedPositionRange position,
		TriviaList leadingTrivia,
		TriviaList trailingTrivia,
		object? value,
		ClassificationList classification)
	{
		Guard.IsOfCategory(kind, SyntaxCategory.Token);

		Kind = kind;
		Lexeme = lexeme;
		Position = position;
		LeadingTrivia = leadingTrivia;
		TrailingTrivia = trailingTrivia;
		Value = value;
		IsFabricated = false;
		Classification = classification;
	}

	/// <summary>Creates a new <see cref="ConcreteSyntaxToken"/> instance from the given <paramref name="token"/>.</summary>
	/// <param name="token">The token produced by the lexer to inherit the values from.</param>
	/// <param name="classification">The classifications for the token.</param>
	public ConcreteSyntaxToken(ITokenNode token, ClassificationList classification)
	{
		Kind = token.Kind;
		Lexeme = token.Lexeme;
		Position = token.Position;
		LeadingTrivia = token.LeadingTrivia;
		TrailingTrivia = token.TrailingTrivia;
		Value = token.Value;
		IsFabricated = false;
		Classification = classification;
	}

	/// <summary>Creates a new, fabricated <see cref="ConcreteSyntaxToken"/> instance.</summary>
	/// <param name="kind">The kind of the token.</param>
	/// <param name="position">The position that the token takes up.</param>
	public ConcreteSyntaxToken(SyntaxKind kind, IndexedPositionRange position)
	{
		Kind = kind;
		Position = position;

		LeadingTrivia = [];
		TrailingTrivia = [];

		IsFabricated = true;
		Classification = [];
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override IEnumerable<IConcreteSyntaxNode> GetChildren() => [LeadingTrivia, TrailingTrivia];
	#endregion
}
