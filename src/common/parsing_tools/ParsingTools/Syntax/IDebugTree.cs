namespace OwlDomain.ParsingTools.Syntax;

public interface IDebugTreeProperty
{
	#region Properties
	string Label { get; }
	object? Value { get; }
	#endregion
}

public interface IDebugTreeObject
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

public interface IDebugTreeList
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
	public void Add(string label, object? value)
	{
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
	public void Add(object? value)
	{
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

	#region Functions
	public static IDebugTree Create(ISyntaxTree tree) => new DebugTreeFactory().Create(tree);
	#endregion
}
