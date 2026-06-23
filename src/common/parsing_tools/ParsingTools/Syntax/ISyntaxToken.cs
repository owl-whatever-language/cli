namespace OwlDomain.ParsingTools.Syntax;

public interface ISyntaxToken : ISyntaxPart
{
	#region Properties
	TriviaList LeadingTrivia { get; }
	TriviaList TrailingTrivia { get; }
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public abstract class BaseSyntaxToken : ISyntaxToken
{
	#region Properties
	public SyntaxNodeKind NodeKind => new(TreeKind, Kind.Name, Kind.Category.Name);
	protected abstract string? TreeKind { get; }
	public abstract int Level { get; }

	/// <inheritdoc/>
	public SyntaxKind Kind { get; }

	/// <inheritdoc/>
	[DisallowNull]
	public ISyntaxNode? Parent
	{
		get;
		set
		{
			if (field is not null)
				ThrowHelper.ThrowInvalidOperationException("The parent node has already been set.");

			field = value;
		}
	}

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

	/// <inheritdoc/>
	public ClassificationKind? Classification { get; }
	#endregion

	#region Constructors
	protected BaseSyntaxToken(
		SyntaxKind kind,
		IndexedPositionRange position,
		string? lexeme,
		object? value,
		TriviaList leadingTrivia,
		TriviaList trailingTrivia,
		ClassificationKind? classification)
	{
		Guard.IsOfCategory(kind, SyntaxCategory.Token);

		Kind = kind;
		Position = position;
		Lexeme = lexeme;
		Value = value;
		LeadingTrivia = leadingTrivia;
		TrailingTrivia = trailingTrivia;
		Classification = classification;
		IsFabricated = false;

		AssignParentToChildren();
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
	private void AssignParentToChildren()
	{
		foreach (ISyntaxNode child in GetChildren())
			child.Parent = this;
	}
	public IEnumerable<ISyntaxNode> GetChildren() => [LeadingTrivia, TrailingTrivia];
	public override string ToString() => this.Print(false);
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"Token {{ Kind = ({Kind}), Lexeme = ({Lexeme}) }}";
	#endregion
}

public sealed class SyntaxToken : BaseSyntaxToken
{
	#region Properties
	protected override string? TreeKind => null;
	public override int Level => 0;
	#endregion

	#region Constructors
	public SyntaxToken(
		SyntaxKind kind,
		IndexedPositionRange position,
		string? lexeme,
		object? value,
		TriviaList leadingTrivia,
		TriviaList trailingTrivia)
		: base(kind, position, lexeme, value, leadingTrivia, trailingTrivia, null)
	{
	}

	public SyntaxToken(
		SyntaxKind kind,
		IndexedPositionRange position,
		string? lexeme,
		object? value)
		: base(kind, position, lexeme, value, TriviaList.Empty, TriviaList.Empty, null)
	{
	}

	public SyntaxToken(SyntaxKind kind, IndexedPositionRange position) : base(kind, position)
	{
	}
	#endregion

	#region Methods
	/// <summary>Creates a new token with the same properties, but with the <paramref name="newLeadingTrivia"/> list instead.</summary>
	/// <param name="newLeadingTrivia">The new leading trivia list.</param>
	/// <returns>A duplicate of the current token with the <paramref name="newLeadingTrivia"/> list.</returns>
	public SyntaxToken ReplaceLeadingTrivia(TriviaList newLeadingTrivia)
	{
		return new(Kind, Position, Lexeme, Value, newLeadingTrivia, TrailingTrivia);
	}
	#endregion
}
