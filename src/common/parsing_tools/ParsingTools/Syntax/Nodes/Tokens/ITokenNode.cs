namespace OwlDomain.ParsingTools.Syntax.Nodes.Tokens;

/// <summary>
/// 	Represents a token node in the concrete syntax tree (CST).
/// </summary>
public interface ITokenNode : IConcreteSyntaxNode
{
	#region Properties
	/// <summary>Whether the token was fabricated during compilation as an error recovery measure.</summary>
	[MemberNotNullWhen(false, nameof(Lexeme))]
	new bool IsFabricated { get; }

	/// <summary>The list of the leading trivia nodes.</summary>
	TriviaList LeadingTrivia { get; }

	/// <summary>The list of the trailing trivia nodes.</summary>
	TriviaList TrailingTrivia { get; }

	/// <summary>The exact input that was parsed for this token.</summary>
	/// <remarks>Fabricated tokens will not have a lexeme.</remarks>
	string? Lexeme { get; }

	/// <summary>The untyped value of the token.</summary>
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
/// 	Represents a token node in the concrete syntax tree (CST).
/// </summary>
/// <typeparam name="T">The type of value that the token can contain.</typeparam>
public interface ITokenNode<out T> : ITokenNode
{
	#region Properties
	/// <summary>The value that the token contains.</summary>
	new T Value { get; }
	object? ITokenNode.Value => Value;
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a token node.
/// </summary>
public abstract class BaseTokenNode : ITokenNode
{
	#region Properties
	/// <inheritdoc/>
	public SyntaxKind Kind { get; }

	/// <inheritdoc/>
	public abstract bool IsFabricated { get; }

	/// <inheritdoc/>
	public TriviaList LeadingTrivia { get; }

	/// <inheritdoc/>
	public TriviaList TrailingTrivia { get; }

	/// <inheritdoc/>
	public abstract string? Lexeme { get; }

	/// <inheritdoc/>
	public IndexedPositionRange FullPosition
	{
		get
		{
			IndexedLinePosition start = LeadingTrivia.Count > 0 ? LeadingTrivia[0].Position.Start : Position.Start;
			IndexedLinePosition end = LeadingTrivia.Count > 0 ? LeadingTrivia[^1].Position.End : Position.End;

			return new(start, end);
		}
	}

	/// <inheritdoc/>
	public IndexedPositionRange Position { get; }
	object? ITokenNode.Value => null;
	#endregion

	#region Constructors
	/// <summary>Populates the properties on the base token node.</summary>
	/// <param name="kind">The kind of the token.</param>
	/// <param name="position">The position that the token takes up.</param>
	/// <param name="leadingTrivia">The list of the leading trivia nodes.</param>
	/// <param name="trailingTrivia">The list of the trailing trivia nodes.</param>
	/// <exception cref="ArgumentException">Thrown if the given syntax <paramref name="kind"/> is not a token.</exception>
	protected BaseTokenNode(
		SyntaxKind kind,
		IndexedPositionRange position,
		TriviaList? leadingTrivia = null,
		TriviaList? trailingTrivia = null)
	{
		Guard.IsOfCategory(kind, SyntaxCategory.Token);

		Kind = kind;
		Position = position;

		LeadingTrivia = leadingTrivia ?? [];
		TrailingTrivia = trailingTrivia ?? [];
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public abstract ITokenNode ReplaceLeadingTrivia(TriviaList newLeadingTrivia);

	/// <inheritdoc/>
	public IEnumerable<IConcreteSyntaxNode> GetChildren() => LeadingTrivia.Concat(TrailingTrivia);

	/// <inheritdoc/>
	public override string ToString() => DebugPrinter.ToString(this);
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a token node.
/// </summary>
/// <typeparam name="T">The type of value that the token can contain.</typeparam>
public abstract class BaseTokenNode<T> : BaseTokenNode, ITokenNode<T>
{
	#region Properties
	/// <inheritdoc/>
	public T Value { get; }
	object? ITokenNode.Value => Value;
	#endregion

	#region Constructors
	/// <summary>Populates the properties on the base token node.</summary>
	/// <param name="kind">The kind of the token.</param>
	/// <param name="position">The position that the token takes up.</param>
	/// <param name="value">The value that the token contains.</param>
	/// <param name="leadingTrivia">The list of the leading trivia nodes.</param>
	/// <param name="trailingTrivia">The list of the trailing trivia nodes.</param>
	/// <exception cref="ArgumentException">Thrown if the given syntax <paramref name="kind"/> is not a token.</exception>
	protected BaseTokenNode(
		SyntaxKind kind,
		IndexedPositionRange position,
		T value,
		TriviaList? leadingTrivia = null,
		TriviaList? trailingTrivia = null)
		: base(kind, position, leadingTrivia, trailingTrivia)
	{
		Value = value;
	}
	#endregion
}
