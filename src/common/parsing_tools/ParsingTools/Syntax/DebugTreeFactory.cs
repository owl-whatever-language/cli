using System.Reflection;

namespace OwlDomain.ParsingTools.Syntax;

public class DebugTreeFactory
{
	#region Nested types
	private sealed class Comparer : Comparer<IDebugTreeProperty>
	{
		public delegate int CompareCallback(IDebugTreeObject target, IDebugTreeProperty left, IDebugTreeProperty right);

		#region Properties
		public IDebugTreeObject Target { get; }
		public CompareCallback Comparison { get; }
		#endregion

		#region Constructors
		public Comparer(IDebugTreeObject target, CompareCallback comparison)
		{
			Target = target;
			Comparison = comparison;
		}
		#endregion

		#region Methods
		public override int Compare(IDebugTreeProperty? x, IDebugTreeProperty? y)
		{
			return (x, y) switch
			{
				(null, null) => 0,
				(null, _) => -1,
				(_, null) => 1,

				_ => Comparison.Invoke(Target, x, y)
			};
		}
		#endregion
	}
	#endregion

	#region Methods
	public virtual IDebugTree Create(ISyntaxTree syntax)
	{
		DebugTree tree = new(syntax);

		Populate(tree, syntax);

		return tree;
	}
	public virtual IDebugTreeObject Create(ISyntaxNode syntax)
	{
		DebugTreeObject obj = new();

		Populate(obj, syntax);

		return obj;
	}

	protected virtual void Populate(DebugTreeObject target, object value)
	{
		Type type = value.GetType();
		PropertyInfo[] properties = type.GetProperties();

		foreach (PropertyInfo property in properties)
		{
			if (Include(property, value) is false)
				continue;

			string label = GetLabel(property);
			object? propertyValue = property.GetValue(value);
			object? node = CreateValue(propertyValue);

			target.Add(label, node);
		}

		PopulateExtra(target, value);
		Prune(target);
		Reorder(target);
		Deduplicate(target);
	}
	protected virtual string GetLabel(PropertyInfo property) => property.Name;
	protected virtual bool Include(PropertyInfo property, object container)
	{
		if (container is ISyntaxNode node)
		{
			if (property.Name is nameof(ISyntaxNode.Parent) or nameof(ISyntaxNode.Position))
				return false;

			if (property.Name is nameof(ISyntaxNode.IsFabricated))
				return node.IsFabricated;
		}

		if (property.Name is "Annotations")
			return false;

		if (container is ISyntaxNode && property.Name is nameof(ISyntaxNode.NodeKind) or nameof(ISyntaxNode.Level))
			return false;

		if (container is ISyntaxTree && property.Name is nameof(ISyntaxTree.Kind) or nameof(ISyntaxTree.Level))
			return false;

		if (container is ISyntaxPart && property.Name is nameof(ISyntaxPart.Lexeme))
			return false;

		if (property.PropertyType == typeof(TriviaList) || property.IsSyntaxPart())
			return false;

		if (container.GetType().IsSeparatedSyntaxListNode() && property.Name is nameof(ISyntaxList<,>.Separators) or nameof(ISyntaxList<,>.Values))
			return false;

		return true;
	}
	protected virtual void Prune(DebugTreeObject target)
	{
		for (int i = target.Properties.Count - 1; i >= 0; i--)
		{
			IDebugTreeProperty property = target.Properties[i];
			if (ShouldPrune(property))
				target.RemoveAt(i);
		}
	}
	protected virtual bool ShouldPrune(IDebugTreeProperty property) => ShouldPrune(property.Value);
	protected virtual void Reorder(DebugTreeObject target)
	{
		Comparer comparer = new(target, Compare);

		int index = 0;
		foreach (IDebugTreeProperty property in target.Properties.Order(comparer))
			target.Move(property, index++);
	}
	protected virtual void Deduplicate(DebugTreeObject target)
	{
		HashSet<string> seen = [];
		Dictionary<string, int> indices = [];

		List<int> toRemove = [];

		for (int i = 0; i < target.Properties.Count; i++)
		{
			IDebugTreeProperty property = target.Properties[i];

			if (property.Value is IDebugTreeObject or IDebugTreeList or null)
				continue;

			// Note(Nightowl): We want to iterate like this because we want to ensure the original order matter;

			string trueValue = property.Value switch
			{
				ISyntaxNode node => node.Print(false), // Note(Nightowl): printer minimised the output;

				_ => property.Value.ToString() ?? "",
			};

			if (seen.Add(trueValue))
				indices[trueValue] = i;
			else
			{
				int originalIndex = indices[trueValue];
				IDebugTreeProperty original = target.Properties[originalIndex];

				// Note(Nightowl): Always replace source with more specific value, at least for now;
				if (original.Label is "Source")
				{
					toRemove.Add(originalIndex);
					indices[trueValue] = i;
				}
				else
					toRemove.Add(i);
			}
		}

		foreach (int index in toRemove)
			target.RemoveAt(index);
	}

	protected virtual int? Compare(bool left, bool right)
	{
		if (left && right)
			return null;
		if (left)
			return -1;
		if (right)
			return 1;

		return null;
	}
	protected bool IsComplex(object? value) => value is IDebugTreeObject or IDebugTreeList;
	protected bool IsSimple(object? value) => IsComplex(value) is false;
	protected virtual int Compare(IDebugTreeObject target, IDebugTreeProperty left, IDebugTreeProperty right)
	{
		return
			Compare(left.Label is "Kind", right.Label is "Kind") ??
			Compare(left.Label is "FullPosition", right.Label is "FullPosition") ??
			Compare(left.Label is "Source", right.Label is "Source") ??
			Compare(IsSimple(left.Value), IsSimple(right.Value)) ??
			target.Properties.IndexOf(left).CompareTo(target.Properties.IndexOf(right));
	}

	protected virtual void Populate(DebugTreeList target, IEnumerable enumerable)
	{
		foreach (object? value in enumerable)
		{
			object? node = CreateValue(value);
			target.Add(node);
		}

		Prune(target);
	}
	protected virtual void Prune(DebugTreeList target)
	{
		for (int i = target.Elements.Count - 1; i >= 0; i--)
		{
			IDebugTreeListElement element = target.Elements[i];
			if (ShouldPrune(element))
				target.RemoveAt(i);
		}
	}
	protected virtual bool ShouldPrune(IDebugTreeListElement element) => ShouldPrune(element.Value);
	protected virtual bool ShouldPrune(object? value)
	{
		return value switch
		{
			null => true,
			IDebugTreeObject obj => ShouldPrune(obj),
			IDebugTreeList list => ShouldPrune(list),

			_ => false,
		};
	}

	protected virtual bool ShouldPrune(IDebugTreeObject obj)
	{
		if (obj.Properties.Count is 0)
			return true;

		HashSet<string> labels = obj.Properties.Select(p => p.Label).ToHashSet();
		labels.Remove("Kind");
		labels.Remove("Source");
		labels.Remove("FullPosition");

		if (labels.Count is 0)
			return true;

		return false;
	}
	protected virtual bool ShouldPrune(IDebugTreeList list)
	{
		if (list.Elements.Count is 0)
			return true;

		return false;
	}

	protected virtual object? CreateValue(object? value)
	{
		if (IsList(value, out IEnumerable? enumerable))
		{
			DebugTreeList list = new();
			Populate(list, enumerable);

			if (list.Elements.Count is 1)
				return list.Elements[0].Value;

			return list;
		}

		if (IsObject(value))
		{
			DebugTreeObject obj = new();
			Populate(obj, value);

			HashSet<string> labels = obj.Properties.Select(p => p.Label).ToHashSet();
			labels.Remove("Kind");
			labels.Remove("FullPosition");
			labels.Remove("Source");

			if (labels.Count is 1)
			{
				IDebugTreeProperty property = obj.Properties.Single(p => p.Label == labels.Single());
				if (property.Value is IDebugTreeObject or IDebugTreeList)
					return property.Value;
			}

			return obj;
		}

		return value;
	}
	protected virtual bool IsList([NotNullWhen(true)] object? value, [NotNullWhen(true)] out IEnumerable? enumerable)
	{
		if (value is string)
		{
			enumerable = default;
			return false;
		}

		enumerable = value as IEnumerable;
		return enumerable is not null;
	}
	protected virtual bool IsObject([NotNullWhen(true)] object? value)
	{
		if (value is ISyntaxTrivia or TriviaList)
			return false;

		if (value is ISyntaxNode)
			return true;

		return false;
	}
	#endregion

	#region Extra property methods
	protected virtual void PopulateExtra(DebugTreeObject target, object value)
	{
		AddKind(target, value);
		AddSource(target, value);
	}
	protected virtual void AddKind(DebugTreeObject target, object value)
	{
		Type type = value.GetType();
		if (type.IsSyntaxListNode() || type.IsSeparatedSyntaxListNode())
			return;

		if (value is not ISyntaxPart)
		{
			if (value is ISyntaxNode node)
				target.Add("Kind", node.NodeKind.WithGroup);
			else if (value is ISyntaxTree tree)
				target.Add("Kind", tree.Kind);
		}
	}
	protected virtual void AddSource(DebugTreeObject target, object value)
	{
		if (value is ISyntaxNode node && node.Position.IsMultiline is false)
			target.Add("Source", node);
	}
	#endregion
}

public static class SyntaxTypeExtensions
{
	extension(Type type)
	{
		#region Methods
		public bool IsSyntaxNode() => type.IsAssignableTo(typeof(ISyntaxNode));
		public bool IsTriviaNode() => type.IsAssignableTo(typeof(ISyntaxTrivia));
		public bool IsTokenNode() => type.IsAssignableTo(typeof(ISyntaxToken));
		public bool IsSyntaxPart() => type.IsAssignableTo(typeof(ISyntaxPart));
		public bool IsSyntaxListNode() => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISyntaxList<>));
		public bool IsSeparatedSyntaxListNode() => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISyntaxList<,>));
		#endregion
	}

	extension(PropertyInfo property)
	{
		#region Methods
		public bool IsSyntaxNode() => property.PropertyType.IsSyntaxNode();
		public bool IsTriviaNode() => property.PropertyType.IsTriviaNode();
		public bool IsTokenNode() => property.PropertyType.IsTokenNode();
		public bool IsSyntaxPart() => property.PropertyType.IsSyntaxPart();
		public bool IsSyntaxListNode() => property.PropertyType.IsSyntaxListNode();
		public bool IsSeparatedSyntaxListNode() => property.PropertyType.IsSeparatedSyntaxListNode();
		#endregion
	}
}
