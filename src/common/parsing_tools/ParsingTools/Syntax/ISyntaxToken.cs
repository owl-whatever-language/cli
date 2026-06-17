namespace OwlDomain.ParsingTools.Syntax;

public interface ISyntaxToken : ISyntaxPart
{
	#region Properties
	TriviaList LeadingTrivia { get; }
	TriviaList TrailingTrivia { get; }
	#endregion
}

public abstract class BaseSyntaxToken : ISyntaxToken
{
	#region Properties
	/// <inheritdoc/>
	public SyntaxKind Kind { get; }

	/// <inheritdoc/>
	public IndexedPositionRange Position { get; }

	/// <inheritdoc/>
	public IndexedPositionRange FullPosition
	{
		get
		{
			ISyntaxNode? first = LeadingTrivia.GetFirstWithAnyPosition();
			ISyntaxNode? last = TrailingTrivia.GetLastFirstWithAnyPosition();

			IndexedLinePosition start = first?.Position.Start ?? Position.Start;
			IndexedLinePosition end = last?.Position.End ?? Position.End;

			return new(start, end);
		}
	}

	/// <inheritdoc/>
	public TriviaList LeadingTrivia { get; }

	/// <inheritdoc/>
	public TriviaList TrailingTrivia { get; }

	/// <inheritdoc/>
	public string? Lexeme { get; }

	/// <inheritdoc/>
	public object? Value { get; }

	/// <inheritdoc/>
	public bool IsFabricated { get; }
	#endregion

	#region Constructors
	protected BaseSyntaxToken(SyntaxKind kind, IndexedPositionRange position, string? lexeme, object? value, TriviaList leadingTrivia, TriviaList trailingTrivia)
	{
		Guard.IsOfCategory(kind, SyntaxCategory.Token);

		Kind = kind;
		Position = position;
		Lexeme = lexeme;
		Value = value;
		LeadingTrivia = leadingTrivia;
		TrailingTrivia = trailingTrivia;
		IsFabricated = false;
	}
	protected BaseSyntaxToken(SyntaxKind kind, IndexedPositionRange position)
	{
		Guard.IsOfCategory(kind, SyntaxCategory.Token);

		Kind = kind;
		Position = position;

		LeadingTrivia = TriviaList.Empty;
		TrailingTrivia = TriviaList.Empty;
		IsFabricated = true;
	}
	#endregion

	#region Methods
	public IEnumerable<ISyntaxNode> GetChildren() => [LeadingTrivia, TrailingTrivia];
	#endregion
}

public sealed class SyntaxToken : BaseSyntaxToken
{
	#region Constructors
	public SyntaxToken(
		SyntaxKind kind,
		IndexedPositionRange position,
		string? lexeme,
		object? value,
		TriviaList leadingTrivia,
		TriviaList trailingTrivia)
		: base(kind, position, lexeme, value, leadingTrivia, trailingTrivia)
	{
	}

	public SyntaxToken(SyntaxKind kind, IndexedPositionRange position) : base(kind, position)
	{
	}
	#endregion
}
