namespace OwlDomain.ParsingTools.Lexing.Trivia;

/// <summary>
/// 	Represents a trivia node in the concrete syntax tree (CST).
/// </summary>
public interface ITriviaNode : IConcreteSyntaxNode
{
	#region Properties
	/// <summary>Whether the trivia node was fabricated during compilation as an error recovery measure.</summary>
	[MemberNotNullWhen(false, nameof(Lexeme))]
	new bool IsFabricated { get; }

	/// <summary>The exact input that was parsed for this trivia node.</summary>
	/// <remarks>Fabricated trivia nodes might not have a lexeme.</remarks>
	string? Lexeme { get; }

	/// <summary>The untyped value of the trivia node.</summary>
	object? Value { get; }

	IndexedPositionRange IConcreteSyntaxNode.FullPosition => Position;
	#endregion
}


/// <summary>
/// 	Represents a trivia node in the concrete syntax tree (CST).
/// </summary>
/// <typeparam name="T">The type of value that the trivia node can contain.</typeparam>
public interface ITriviaNode<T> : ITriviaNode
{
	#region Properties
	/// <summary>The value that the token contains.</summary>
	new T Value { get; }
	object? ITriviaNode.Value => Value;
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a trivia node.
/// </summary>
public abstract class BaseTriviaNode : ITriviaNode
{
	#region Properties
	/// <inheritdoc/>
	public SyntaxKind Kind { get; }

	/// <inheritdoc/>
	public IndexedPositionRange Position { get; }

	/// <inheritdoc/>
	public abstract bool IsFabricated { get; }

	/// <inheritdoc/>
	public abstract string? Lexeme { get; }
	object? ITriviaNode.Value => null;
	#endregion

	#region Constructors
	/// <summary>Populates the properties on the base trivia node.</summary>
	/// <param name="kind">The kind of the trivia node.</param>
	/// <param name="position">The position that the trivia node takes up.</param>
	/// <exception cref="ArgumentException">Thrown if the given syntax <paramref name="kind"/> is not a trivia node.</exception>
	protected BaseTriviaNode(
		SyntaxKind kind,
		IndexedPositionRange position)
	{
		Guard.IsOfCategory(kind, SyntaxCategory.Trivia);

		Kind = kind;
		Position = position;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public virtual IEnumerable<IConcreteSyntaxNode> GetChildren() => [];

	/// <inheritdoc/>
	public override string ToString() => DebugPrinter.ToString(this);
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a trivia node.
/// </summary>
/// <typeparam name="T">The type of value that the trivia can contain.</typeparam>
public abstract class BaseTriviaNode<T> : BaseTriviaNode, ITriviaNode<T>
{
	#region Properties
	/// <inheritdoc/>
	public T Value { get; }
	object? ITriviaNode.Value => Value;
	#endregion

	#region Constructors
	/// <summary>Populates the properties on the base trivia node.</summary>
	/// <param name="kind">The kind of the trivia node.</param>
	/// <param name="position">The position that the trivia node takes up.</param>
	/// <param name="value">The value that the trivia node contains.</param>
	/// <exception cref="ArgumentException">Thrown if the given syntax <paramref name="kind"/> is not a trivia node.</exception>
	protected BaseTriviaNode(
		SyntaxKind kind,
		IndexedPositionRange position,
		T value)
		: base(kind, position)
	{
		Value = value;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override IEnumerable<IConcreteSyntaxNode> GetChildren()
	{
		if (Value is IConcreteSyntaxNode node)
			return [node];

		return [];
	}
	#endregion
}
