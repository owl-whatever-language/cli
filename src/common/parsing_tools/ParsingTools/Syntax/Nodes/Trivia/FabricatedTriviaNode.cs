namespace OwlDomain.ParsingTools.Syntax.Nodes.Trivia;

/// <summary>
/// 	Represents a trivia node that was fabricated as an error recovery measure.
/// </summary>
public sealed class FabricatedTriviaNode : BaseTriviaNode
{
	#region Properties
	/// <inheritdoc/>
	public override bool IsFabricated => true;

	/// <inheritdoc/>
	public override string? Lexeme { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new fabricated trivia node.</summary>
	/// <param name="kind">The kind of the trivia node.</param>
	/// <param name="position">The position that the trivia node takes up.</param>
	/// <param name="lexeme">The exact input that was parsed for this trivia node.</param>
	/// <exception cref="ArgumentException">Thrown if the given syntax <paramref name="kind"/> is not a trivia node.</exception>
	public FabricatedTriviaNode(
		SyntaxKind kind,
		IndexedPositionRange position,
		string? lexeme = null)
		: base(kind, position)
	{
		Lexeme = lexeme;
	}
	#endregion
}

/// <summary>
/// 	Represents a trivia node that was fabricated as an error recovery measure.
/// </summary>
/// <typeparam name="T">The type of value that the trivia node can contain.</typeparam>
public sealed class FabricatedTriviaNode<T> : BaseTriviaNode<T>
{
	#region Properties
	/// <inheritdoc/>
	public override bool IsFabricated => true;

	/// <inheritdoc/>
	public override string? Lexeme { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new fabricated trivia node.</summary>
	/// <param name="kind">The kind of the trivia node.</param>
	/// <param name="position">The position that the trivia node takes up.</param>
	/// <param name="value">The value that the trivia node contains.</param>
	/// <param name="lexeme">The exact input that was parsed for this trivia node.</param>
	/// <exception cref="ArgumentException">Thrown if the given syntax <paramref name="kind"/> is not a trivia node.</exception>
	public FabricatedTriviaNode(
		SyntaxKind kind,
		IndexedPositionRange position,
		T value,
		string? lexeme = null)
		: base(kind, position, value)
	{
		Lexeme = lexeme;
	}
	#endregion
}
