namespace OwlDomain.ParsingTools.Syntax;

/// <summary>
/// 	Represents a syntax node.
/// </summary>
public interface ISyntaxNode
{
	#region Properties
	/// <summary>The kind of the trivia node.</summary>
	/// <remarks>This syntax kind will belong to the <see cref="SyntaxCategory.Trivia"/>.</remarks>
	SyntaxKind Kind { get; }

	/// <summary>The position that the syntax node takes up in the source.</summary>
	IndexedPositionRange Position { get; }
	#endregion

	#region Methods
	/// <summary>Gets the direct child syntax nodes.</summary>
	/// <returns>An enumerable of the direct children.</returns>
	IEnumerable<ISyntaxNode> GetChildren();
	#endregion
}

/// <summary>
/// 	Represents a syntax node.
/// </summary>
/// <typeparam name="T">The type of the syntax nodes that are present in the tree.</typeparam>
public interface ISyntaxNode<T> : ISyntaxNode
	where T : class, ISyntaxNode
{
	#region Methods
	/// <summary>Gets the direct child syntax nodes.</summary>
	/// <returns>An enumerable of the direct children.</returns>
	new IEnumerable<T> GetChildren();
	IEnumerable<ISyntaxNode> ISyntaxNode.GetChildren() => GetChildren();
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a syntax node.
/// </summary>
public abstract class BaseSyntaxNode<T> : ISyntaxNode<T>
	where T : class, ISyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public abstract SyntaxKind Kind { get; }

	/// <inheritdoc/>
	public virtual IndexedPositionRange Position
	{
		get
		{
			ISyntaxNode? first = GetChildren().FirstOrDefault();
			if (first is null)
				return default;

			ISyntaxNode last = GetChildren().Last();

			return new(first.Position.Start, last.Position.End);
		}
	}

	/// <summary>Whether the full syntax node is fabricated.</summary>
	/// <remarks>This is only valid if the node returns children of the <see cref="IConcreteSyntaxNode"/> type.</remarks>
	public virtual bool IsFabricated => GetChildren().OfType<IConcreteSyntaxNode>().All(c => c.IsFabricated);

	/// <summary>
	/// 	The full position that the syntax node takes up in the source.
	///	The full position includes the leading and trailing trivia.
	/// </summary>
	/// <remarks>This is only valid if the node returns children of the <see cref="IConcreteSyntaxNode"/> type.</remarks>
	public virtual IndexedPositionRange FullPosition
	{
		get
		{
			ISyntaxNode? first = GetChildren().FirstOrDefault();
			if (first is null)
				return default;

			ISyntaxNode last = GetChildren().Last();

			IndexedLinePosition start = first is IConcreteSyntaxNode firstConcrete ? firstConcrete.FullPosition.Start : first.Position.Start;
			IndexedLinePosition end = last is IConcreteSyntaxNode lastConcrete ? lastConcrete.FullPosition.End : last.Position.End;

			return new(start, end);
		}
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public abstract IEnumerable<T> GetChildren();

	/// <inheritdoc/>
	public override string? ToString()
	{
		if (this is IConcreteSyntaxNode node)
			return DebugPrinter.ToString(node);

		return base.ToString();
	}
	#endregion
}

/// <summary>
/// 	Contains various extensions for adding syntax node related guard functions.
/// </summary>
public static class SyntaxNodeGuards
{
	extension(Guard)
	{
		#region Functions
		/// <summary>Asserts that the given syntax node <paramref name="list"/> is in order.</summary>
		/// <param name="list">The list to check.</param>
		/// <param name="name">The name of the input parameter being tested.</param>
		/// <exception cref="ArgumentException">Thrown if the given <paramref name="list"/> is not ordered.</exception>
		public static void IsOrdered(IReadOnlyList<ISyntaxNode> list, [CallerArgumentExpression(nameof(list))] string name = "")
		{
			if (list.Count < 2)
				return;

			IndexedLinePosition last = list[0].Position.Start;

			for (int i = 1; i < list.Count; i++)
			{
				IndexedLinePosition current = list[i].Position.Start;
				if (current < last)
					ThrowHelper.ThrowArgumentException(name, $"The node at index {i} is out of order.");

				last = current;
			}
		}
		#endregion
	}
}
