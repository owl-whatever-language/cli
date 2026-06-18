namespace OwlDomain.ParsingTools.Syntax;

public interface ISyntaxNode
{
	#region Properties
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
	/// <inheritdoc/>
	public IndexedPositionRange Position => GetChildren().GetPosition();

	/// <inheritdoc/>
	public IndexedPositionRange FullPosition => GetChildren().GetFullPosition();

	/// <inheritdoc/>
	public bool IsFabricated => GetChildren().OrderByDescending(static c => c is ISyntaxToken).All(static c => c.IsFabricated);
	#endregion

	#region Methods
	public abstract IEnumerable<ISyntaxNode> GetChildren();
	public override string ToString() => this.Print();
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"{GetType().Name}: {this.Print(false)}";
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
		#region Methods
		public IReadOnlyList<ISyntaxToken> Flatten() => Flatten<ISyntaxToken>(node);
		public IReadOnlyList<T> Flatten<T>() where T : notnull, ISyntaxNode
		{
			List<T> target = [];
			Flatten(target, node);

			return target;
		}
		private static void Flatten<T>(List<T> target, ISyntaxNode current)
		{
			if (current is T typed)
			{
				target.Add(typed);
				return;
			}

			foreach (ISyntaxNode child in current.GetChildren())
				Flatten(target, child);
		}
		public string Print(bool includeStartAndEndTrivia = true)
		{
			StringBuilder builder = new();

			ISyntaxToken? last = null;

			foreach (ISyntaxToken token in Flatten(node))
			{
				if (last is not null)
				{
					foreach (ISyntaxTrivia trivia in last.TrailingTrivia)
						builder.Append(trivia.Lexeme);
				}

				if (includeStartAndEndTrivia || last != null)
				{
					foreach (ISyntaxTrivia trivia in token.LeadingTrivia)
						builder.Append(trivia.Lexeme);
				}

				builder.Append(token.Lexeme);
				last = token;
			}

			if (includeStartAndEndTrivia && last is not null)
			{
				foreach (ISyntaxTrivia trivia in last.TrailingTrivia)
					builder.Append(trivia.Lexeme);
			}

			string text = builder.ToString();
			return text;
		}
		#endregion
	}
}
