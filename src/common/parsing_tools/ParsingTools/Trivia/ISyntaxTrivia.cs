namespace OwlDomain.ParsingTools.Trivia;

public interface ISyntaxTrivia : ISyntaxPart
{
	#region Properties
	ClassificationKind? Classification { get; }
	#endregion
}

public sealed class SyntaxTrivia : ISyntaxTrivia
{
	#region Properties
	/// <inheritdoc/>
	public SyntaxKind Kind { get; }

	/// <inheritdoc/>
	public IndexedPositionRange Position { get; }

	/// <inheritdoc/>
	public IndexedPositionRange FullPosition { get; }

	/// <inheritdoc/>
	public string? Lexeme { get; }

	/// <inheritdoc/>
	public object? Value { get; }

	/// <inheritdoc/>
	public bool IsFabricated { get; }

	/// <inheritdoc/>
	public ClassificationKind? Classification { get; }
	#endregion

	#region Constructors
	public SyntaxTrivia(SyntaxKind kind, IndexedPositionRange position, string lexeme, ClassificationKind? classification = null, object? value = null)
	{
		Guard.IsOfCategory(kind, SyntaxCategory.Trivia);

		Kind = kind;
		Position = position;
		FullPosition = position;

		Lexeme = lexeme;
		Classification = classification;
		Value = value;
		IsFabricated = false;
	}
	public SyntaxTrivia(SyntaxKind kind, IndexedPositionRange position)
	{
		Guard.IsOfCategory(kind, SyntaxCategory.Trivia);

		Kind = kind;
		Position = position;
		FullPosition = position;

		IsFabricated = true;
	}
	public SyntaxTrivia(ISyntaxNode badSyntax)
	{
		Kind = SyntaxKind.BadSyntax;

		Position = badSyntax.Position;
		FullPosition = badSyntax.FullPosition;

		IsFabricated = true;
	}
	#endregion

	#region Methods
	public IEnumerable<ISyntaxNode> GetChildren()
	{
		if (Value is ISyntaxNode node)
			return [node];

		return [];
	}
	#endregion
}
