namespace OwlDomain.ParsingTools.Syntax;

public interface ISyntaxNode : IDebugTreePrintable
{
	#region Properties
	SyntaxNodeKind NodeKind { get; }
	int Level { get; }

	[DisallowNull]
	ISyntaxNode? Parent { get; set; }
	IndexedPositionRange Position { get; }
	IndexedPositionRange FullPosition { get; }
	bool IsFabricated { get; }
	#endregion

	#region Methods
	IEnumerable<ISyntaxNode> GetChildren();
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public abstract class BaseSyntaxNode : ISyntaxNode
{
	#region Properties
	public abstract SyntaxNodeKind NodeKind { get; }
	public abstract int Level { get; }

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
	public IndexedPositionRange Position => GetChildren().GetPosition();

	/// <inheritdoc/>
	public IndexedPositionRange FullPosition => GetChildren().GetFullPosition();

	/// <inheritdoc/>
	public bool IsFabricated => GetChildren().OrderByDescending(static c => c is ISyntaxToken).All(static c => c.IsFabricated);
	#endregion

	#region Methods
	protected void AssignParentToChildren()
	{
		foreach (ISyntaxNode child in GetChildren())
			child.Parent = this;
	}
	public abstract IEnumerable<ISyntaxNode> GetChildren();
	TextFragmentCollection IDebugTreePrintable.GetFragments() => this.GetDebugFragments();
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"{GetType().Name}: {this.GetDebugSource()}";
	#endregion
}

public static class ISyntaxNodeExtensions
{
	extension(IEnumerable<ISyntaxNode> enumerable)
	{
		#region Position methods
		public ISyntaxNode? GetFirstWithPosition() => enumerable.FirstOrDefault(s => s.Position != default);
		public ISyntaxNode? GetLastFirstWithPosition() => enumerable.LastOrDefault(s => s.Position != default);
		public ISyntaxNode? GetFirstWithAnyPosition() => enumerable.FirstOrDefault(s => s.FullPosition != default || s.Position != default);
		public ISyntaxNode? GetLastFirstWithAnyPosition() => enumerable.LastOrDefault(s => s.FullPosition != default || s.Position != default);
		public IndexedPositionRange GetPosition()
		{
			ISyntaxNode? first = GetFirstWithPosition(enumerable);
			if (first is null)
				return default;

			ISyntaxNode? last = GetLastFirstWithPosition(enumerable) ?? first;

			return new(first.Position.Start, last.Position.End);
		}
		public IndexedPositionRange GetFullPosition()
		{
			ISyntaxNode? first = GetFirstWithAnyPosition(enumerable);
			if (first is null)
				return default;

			ISyntaxNode? last = GetLastFirstWithAnyPosition(enumerable) ?? first;

			return new(first.FullPosition.Start, last.FullPosition.End);
		}
		#endregion
	}

	extension(ISyntaxNode node)
	{
		#region Parent methods
		public T? GetParent<T>(bool includeSelf = true) where T : notnull, ISyntaxNode
		{
			ISyntaxNode? current = includeSelf ? node : node.Parent;

			while (current is not null)
			{
				if (current is T typed)
					return typed;

				current = current.Parent;
			}

			return default;
		}
		public ISyntaxNode GetRoot()
		{
			while (node.Parent is not null)
				node = node.Parent;

			return node;
		}
		public ISyntaxDocument GetDocument()
		{
			ISyntaxNode root = node.GetRoot();
			if (root is ISyntaxDocument document)
				return document;

			ThrowHelper.ThrowInvalidOperationException($"Expected the root of the node to be a document, calling this method before the tree is build is not allowed.");
			return default;
		}
		public T GetDocument<T>() where T : notnull, ISyntaxDocument => (T)node.GetDocument();
		public ISyntaxTree GetTree()
		{
			ISyntaxDocument document = node.GetDocument();

			if (document.Tree is null)
				ThrowHelper.ThrowInvalidOperationException("Expected the tree to be accessible.");

			return document.Tree;
		}
		public T GetTree<T>() where T : notnull, ISyntaxTree => (T)node.GetTree();
		#endregion

		#region Search methods
		public ISyntaxNode? Search(Predicate<ISyntaxNode> predicate, bool includeSelf = true) => Search<ISyntaxNode>(node, predicate, includeSelf);
		public T? Search<T>(Predicate<T> predicate, bool includeSelf = true) where T : notnull, ISyntaxNode
		{
			return Search(predicate, node, includeSelf);
		}
		private static T? Search<T>(Predicate<T> predicate, ISyntaxNode current, bool includeCurrent) where T : notnull, ISyntaxNode
		{
			if (includeCurrent && current is T typed)
			{
				if (predicate.Invoke(typed))
					return typed;
			}

			foreach (ISyntaxNode child in current.GetChildren())
			{
				T? result = Search(predicate, child, includeCurrent: true);
				if (result is not null)
					return result;
			}

			return default;
		}
		#endregion

		#region Flatten methods
		public IReadOnlyList<ISyntaxToken> ToTokens() => Flatten<ISyntaxToken>(node);
		public IReadOnlyList<ISyntaxPart> ToParts() => Flatten<ISyntaxPart>(node);
		public IEnumerable<ISyntaxPart> ToPartsWithoutOuterTrivia()
		{
			IReadOnlyList<ISyntaxPart> parts = node.ToParts();

			int start = parts.FindIndex(static p => p is ISyntaxToken);
			if (start < 0)
				return [];

			int last = parts.FindLastIndex(static p => p is ISyntaxToken);
			Debug.Assert(last >= 0);

			int amount = last - start + 1;

			return parts.Skip(start).Take(amount);
		}
		public IReadOnlyList<T> Flatten<T>() where T : notnull, ISyntaxNode
		{
			List<T> target = [];
			Flatten(target, node);

			// Note(Nightowl):
			// I think sorting is required because adding them in the correct order is far
			// too annoying on account of bad syntax in nested trivia vs node locality;
			target.Sort((a, b) => a.Position.Start.CompareTo(b.Position.Start));

			return target;
		}
		private static void Flatten<T>(List<T> target, ISyntaxNode current)
		{
			if (current is T typed)
				target.Add(typed);

			foreach (ISyntaxNode child in current.GetChildren())
				Flatten(target, child);
		}
		#endregion

		#region Kind and level validation methods
		public void ThrowIfInvalidShadow(ISyntaxNode @new, [CallerArgumentExpression(nameof(@new))] string? parameter = null)
		{
			if (node.NodeKind.WithGroup != @new.NodeKind.WithGroup)
				ThrowHelper.ThrowArgumentException(parameter, $"The current node ({node.NodeKind.WithGroup}) tried to be shadowed by a node with a different kind ({@new.NodeKind.WithGroup}).");

			if (node.Level >= @new.Level)
				ThrowHelper.ThrowArgumentException(parameter, $"The current node ({node.Level}: {node.NodeKind.Kind}) tried to be shadowed by a node with a different level ({@new.Level}: {@new.NodeKind.Kind}).");
		}
		#endregion
	}
}
