namespace OwlDomain.ParsingTools.Trivia;

public interface ISyntaxTrivia : ISyntaxPart, IDebugObjectFactory
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	ISyntaxNode? ISyntaxNode.ShadowedBy
	{
		get => null;
		set => ThrowHelper.ThrowInvalidOperationException<ISyntaxNode>("Syntax trivia is never shadowed.");
	}
}

public interface IBadSyntaxTrivia : ISyntaxTrivia
{
	#region Properties
	ISyntaxNode BadSyntax { get; set; }
	#endregion
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
	#endregion

	#region Methods
	public IEnumerable<ISyntaxNode> GetChildren() => [];
	public IDebugTreeObject GetDebugObject()
	{
		DebugTreeObject obj = new();

		obj.Add(nameof(Kind), Kind);

		if (IsFabricated)
			obj.Add(nameof(IsFabricated), true);
		else if (Lexeme is not null)
			obj.Add(nameof(Lexeme), Lexeme, Classification);

		if (Classification is not null)
			obj.Add(nameof(Classification), Classification);

		return obj;
	}
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"Trivia({Kind}): {Lexeme}";
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class BadSyntaxTrivia : IBadSyntaxTrivia
{
	#region Properties
	public SyntaxNodeKind NodeKind => new(null, Kind.Name, Kind.Category.Name);
	public int Level => 0;

	/// <inheritdoc/>
	public SyntaxKind Kind => SyntaxKind.BadSyntax;

	/// <inheritdoc/>
	[DisallowNull]
	public ISyntaxNode? Parent { get; set; }

	/// <inheritdoc/>
	public IndexedPositionRange Position => BadSyntax.Position;

	/// <inheritdoc/>
	public IndexedPositionRange FullPosition => BadSyntax.FullPosition;

	/// <inheritdoc/>
	public string? Lexeme => null;

	/// <inheritdoc/>
	public object? Value => null;

	/// <inheritdoc/>
	public bool IsFabricated => true;

	/// <inheritdoc/>
	public ClassificationKind? Classification => null;

	/// <inheritdoc/>
	public ISyntaxNode BadSyntax
	{
		get;
		set
		{
			if (field is not null) // Note(Nightowl): First set from the constructor;
			{
				if (value.NodeKind.WithGroup != field.NodeKind.WithGroup)
					ThrowHelper.ThrowArgumentException(nameof(value), "The bad syntax can only be shadowed by a node of the same kind.");

				if (value.Level <= field.Level)
					ThrowHelper.ThrowArgumentException(nameof(value), "The bad syntax can only be shadowed by a node with a higher level.");
			}

			field = value;
		}
	}
	#endregion

	#region Constructors
	public BadSyntaxTrivia(ISyntaxNode badSyntax)
	{
		BadSyntax = badSyntax;

		// Note(Nightowl): We can't (yet) replace it, however it's necessary for a lot of error reporting at the moment;
		badSyntax.Parent = this;
	}
	#endregion

	#region Methods
	public IEnumerable<ISyntaxNode> GetChildren() => [BadSyntax];
	public IDebugTreeObject GetDebugObject()
	{
		DebugTreeObject obj = new();

		obj.Add(nameof(Kind), Kind);
		obj.Add(nameof(BadSyntax), BadSyntax);

		return obj;
	}
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"{nameof(BadSyntaxTrivia)}({BadSyntax.NodeKind})";
	#endregion
}
