namespace OwlDomain.ParsingTools.Syntax.Nodes;

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
