namespace OwlDomain.ParsingTools.Text.Fragments;

public interface ITextFragmentCollection : IReadOnlyList<TextFragment>
{
	#region Properties
	bool IsWhitespace { get; }
	#endregion
}

public class TextFragmentCollection : List<TextFragment>, ITextFragmentCollection
{
	#region Properties
	public bool IsWhitespace => this.All(static f => f.IsWhitespace);
	#endregion

	#region Constructors
	public TextFragmentCollection() { }
	public TextFragmentCollection(IEnumerable<TextFragment> collection) : base(collection) { }
	public TextFragmentCollection(int capacity) : base(capacity) { }
	#endregion

	#region Methods
	public void Add(string text, ClassificationKind? classification, ISyntaxPart? syntax = null)
	{
		TextFragment fragment = new(text, classification, syntax);
		Add(fragment);
	}
	public bool TryAdd(ISyntaxPart part)
	{
		if (part.Lexeme is null)
			return false;

		TextFragment fragment = new(part.Lexeme, part.Classification, part);
		Add(fragment);

		return true;
	}
	public void AddRange(IDebugNodeFactory<IDebugTreeText> factory)
	{
		TextFragmentCollection collection = factory.GetDebugNode().Fragments;
		AddRange(collection);
	}
	public void TrimEnd()
	{
		while (Count > 0)
		{
			TextFragment last = this[^1];
			if (last.IsWhitespace is false)
				break;

			RemoveAt(Count - 1);
		}
	}
	public override string ToString() => string.Concat(this);
	#endregion
}
