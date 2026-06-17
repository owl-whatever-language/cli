namespace OwlDomain.ParsingTools.Trivia;

public interface ISyntaxTrivia : ISyntaxPart
{
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
	#endregion

	#region Constructors
	public SyntaxTrivia(SyntaxKind kind, IndexedPositionRange position, string lexeme, object? value = null)
	{
		Guard.IsOfCategory(kind, SyntaxCategory.Trivia);

		Kind = kind;
		Position = position;
		FullPosition = position;

		Lexeme = lexeme;
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
