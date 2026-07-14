using System.CodeDom.Compiler;

namespace OwlDomain.ParsingTools.Text.Fragments;

public interface ITextFragmentCollection : IReadOnlyList<TextFragment>, IPlainTextPrintable
{
	#region Properties
	bool IsWhitespace { get; }
	#endregion
}

public class TextFragmentCollection : List<TextFragment>, ITextFragmentCollection
{
	#region Properties
	public bool IsWhitespace => this.All(static f => f.IsWhitespace);
	public bool IsSourcePrefix => this.All(static f => f.IsSourcePrefix);
	public bool IsWhiteSpaceOrSourcePrefix => this.All(static f => f.IsWhiteSpaceOrSourcePrefix);
	public bool IsOnlyCommented
	{
		get
		{
			if (this.Any(f => f.IsComment))
				return this.All(f => f.IsComment || f.IsWhitespace);

			return false;
		}
	}
	public bool IsDimmed => this.All(static f => f.IsDimmed);
	public bool IsSnipped => this.All(static f => f.IsWhiteSpaceOrSourcePrefix || f.IsSnipped);
	#endregion

	#region Constructors
	public TextFragmentCollection() { }
	public TextFragmentCollection(params IEnumerable<TextFragment> collection) : base(collection) { }
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
	public void WritePlainText(IndentedTextWriter writer)
	{
		foreach (TextFragment fragment in this)
			writer.Write(fragment.Text);
	}
	public override string ToString() => string.Concat(this);
	#endregion
}
