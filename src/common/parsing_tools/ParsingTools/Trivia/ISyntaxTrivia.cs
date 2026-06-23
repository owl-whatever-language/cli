namespace OwlDomain.ParsingTools.Trivia;

public interface ISyntaxTrivia : ISyntaxPart
{
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class SyntaxTrivia : ISyntaxTrivia
{
	#region Properties
	public SyntaxNodeKind NodeKind => new(null, Kind.Name, Kind.Category.Name);
	public int Level => 0;

	/// <inheritdoc/>
	public SyntaxKind Kind { get; }

	/// <inheritdoc/>
	[DisallowNull]
	public ISyntaxNode? Parent { get; set; }

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
		Value = badSyntax;

		IsFabricated = true;

		// Note(Nightowl): We purposefully don't assign the badSyntax's parent because we won't be able to replace it;
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

	#region Helpers
	private string DebuggerDisplay() => $"Trivia({Kind}): {Lexeme}";
	#endregion
}
