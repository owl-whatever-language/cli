namespace OwlDomain.ParsingTools.Syntax.Nodes;

/// <summary>
/// 	Represents a syntax node in the concrete syntax tree (CST).
/// </summary>
public interface IConcreteSyntaxNode : ISyntaxNode
{
	#region Properties
	/// <summary>The full position that the syntax node takes up in the source.</summary>
	/// <remarks>The full position includes the leading and trailing trivia.</remarks>
	IndexedPositionRange FullPosition { get; }
	#endregion

	#region Methods
	/// <summary>Gets the direct child syntax nodes.</summary>
	/// <returns>An enumerable of the direct children.</returns>
	new IEnumerable<IConcreteSyntaxNode> GetChildren();
	IEnumerable<ISyntaxNode> ISyntaxNode.GetChildren() => GetChildren();
	#endregion
}

/// <summary>
/// 	Represents the base implementation for a concrete syntax node.
/// </summary>
public abstract class BaseConcreteSyntaxNode : IConcreteSyntaxNode
{
	#region Properties
	/// <inheritdoc/>
	public abstract SyntaxKind Kind { get; }

	/// <inheritdoc/>
	public IndexedPositionRange FullPosition
	{
		get
		{
			ISyntaxNode? first = GetChildren().FirstOrDefault();
			if (first is null)
				return default;

			ISyntaxNode last = GetChildren().Last();

			if (first is not IConcreteSyntaxNode firstTyped)
			{
				ThrowHelper.ThrowInvalidOperationException($"A concrete syntax node is expected to only have other concrete syntax nodes, but had {first.GetType()} instead.");
				return default;
			}
			else if (last is not IConcreteSyntaxNode lastTyped)
			{
				ThrowHelper.ThrowInvalidOperationException($"A concrete syntax node is expected to only have other concrete syntax nodes, but had {last.GetType()} instead.");
				return default;
			}
			else
				return new(firstTyped.FullPosition.Start, lastTyped.FullPosition.End);
		}
	}

	/// <inheritdoc/>
	public IndexedPositionRange Position
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
	#endregion

	#region Methods
	/// <inheritdoc/>
	public abstract IEnumerable<IConcreteSyntaxNode> GetChildren();

	/// <inheritdoc/>
	public override string ToString() => DebugPrinter.ToString(this);
	#endregion
}
