namespace OwlDomain.ParsingTools.Syntax.Nodes.Trivia;

/// <summary>
/// 	Represents a regular trivia node.
/// </summary>
public sealed class TriviaNode : BaseTriviaNode
{
	#region Properties
	/// <inheritdoc/>
	public override bool IsFabricated => false;

	/// <inheritdoc/>
	public override string Lexeme { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new trivia node.</summary>
	/// <param name="kind">The kind of the trivia node.</param>
	/// <param name="position">The position that the trivia node takes up.</param>
	/// <param name="lexeme">The exact input that was parsed for this trivia node.</param>
	/// <exception cref="ArgumentException">Thrown if the given syntax <paramref name="kind"/> is not a trivia node.</exception>
	public TriviaNode(
		SyntaxKind kind,
		IndexedPositionRange position,
		string lexeme)
		: base(kind, position)
	{
		Lexeme = lexeme;
	}
	#endregion
}

/// <summary>
/// 	Represents a regular trivia node.
/// </summary>
/// <typeparam name="T">The type of value that the trivia node can contain.</typeparam>
public sealed class TriviaNode<T> : BaseTriviaNode<T>
{
	#region Properties
	/// <inheritdoc/>
	public override bool IsFabricated => true;

	/// <inheritdoc/>
	public override string Lexeme { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new trivia node.</summary>
	/// <param name="kind">The kind of the trivia node.</param>
	/// <param name="position">The position that the trivia node takes up.</param>
	/// <param name="lexeme">The exact input that was parsed for this trivia node.</param>
	/// <param name="value">The value that the trivia node contains.</param>
	/// <exception cref="ArgumentException">Thrown if the given syntax <paramref name="kind"/> is not a trivia node.</exception>
	public TriviaNode(
		SyntaxKind kind,
		IndexedPositionRange position,
		string lexeme,
		T value)
		: base(kind, position, value)
	{
		Lexeme = lexeme;
	}
	#endregion
}
