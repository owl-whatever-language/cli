namespace OwlDomain.ParsingTools.Syntax.Debugging;

public interface IDebugTreeNode { }

public interface IDebugTreeText : IDebugTreeNode
{
	#region Properties
	TextFragmentCollection Fragments { get; }
	#endregion
}

public interface IDebugTreeProperty
{
	#region Properties
	string Label { get; }
	object? Value { get; }
	#endregion
}

public interface IDebugTreeObject : IDebugTreeNode
{
	#region Properties
	IReadOnlyList<IDebugTreeProperty> Properties { get; }
	#endregion
}

public interface IDebugTreeListElement
{
	#region Properties
	int Index { get; }
	object? Value { get; }
	#endregion
}

public interface IDebugTreeList : IDebugTreeNode
{
	#region Properties
	IReadOnlyList<IDebugTreeListElement> Elements { get; }
	#endregion
}

public interface IDebugTree : IDebugTreeObject
{
	#region Properties
	ISyntaxTree Tree { get; }
	#endregion
}

public sealed class DebugTreeText : IDebugTreeText
{
	#region Properties
	public TextFragmentCollection Fragments { get; }
	#endregion

	#region Constructors
	public DebugTreeText(params TextFragmentCollection fragments) => Fragments = fragments;
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class DebugTreeProperty : IDebugTreeProperty
{
	#region Properties
	public string Label { get; }
	public object? Value { get; }
	#endregion

	#region Constructors
	public DebugTreeProperty(string label, object? value)
	{
		Label = label;
		Value = value;
	}
	#endregion

	#region Methods
	private string DebuggerDisplay() => $"{Label}: {Value}";
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class DebugTreeListElement : IDebugTreeListElement
{
	#region Properties
	public int Index { get; }
	public object? Value { get; }
	#endregion

	#region Constructors
	public DebugTreeListElement(int index, object? value)
	{
		Index = index;
		Value = value;
	}
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"{Index}: {Value}";
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public class DebugTreeObject : IDebugTreeObject
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<IDebugTreeProperty> _properties = [];
	#endregion

	#region Properties
	public IReadOnlyList<IDebugTreeProperty> Properties => _properties;
	#endregion

	#region Methods
	public void Add(string label, string text, ClassificationKind? classification = null)
	{
		TextFragment fragment = new(text, classification);
		Add(label, fragment);
	}
	public void Add(string label, TextFragment fragment) => Add(label, [fragment]);
	public void Add(string label, params TextFragmentCollection fragments)
	{
		DebugTreeText text = new(fragments);
		Add(label, text);
	}
	public void Add(string label, IDebugNodeFactory factory)
	{
		IDebugTreeNode node = factory.GetDebugNode();
		Add(label, node);
	}
	public void Add(string label, object? value)
	{
		if (value is IDebugNodeFactory factory)
			value = factory.GetDebugNode();

		DebugTreeProperty property = new(label, value);
		_properties.Add(property);
	}
	public void RemoveAt(int index) => _properties.RemoveAt(index);
	public void Move(IDebugTreeProperty property, int index) => _properties[index] = property;
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"Object {{ Count = ({Properties.Count:n0}) }}";
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class DebugTreeList : IDebugTreeList
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<IDebugTreeListElement> _elements = [];
	#endregion

	#region Properties
	public IReadOnlyList<IDebugTreeListElement> Elements => _elements;
	#endregion

	#region Methods
	public void Add(IDebugNodeFactory factory)
	{
		IDebugTreeNode node = factory.GetDebugNode();
		Add(node);
	}
	public void Add(object? value)
	{
		if (value is IDebugNodeFactory factory)
			value = factory.GetDebugNode();

		int index = _elements.Count;
		DebugTreeListElement element = new(index, value);
		_elements.Add(element);
	}
	public void RemoveAt(int index) => _elements.RemoveAt(index);
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"List {{ Count = ({Elements.Count:n0}) }}";
	#endregion
}

public sealed class DebugTree : DebugTreeObject, IDebugTree
{
	#region Properties
	public ISyntaxTree Tree { get; }
	#endregion

	#region Constructors
	public DebugTree(ISyntaxTree tree) => Tree = tree;
	#endregion
}
