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
	}
	protected virtual void PopulateExtra(DebugTreeObject target, object value)
	{
		if (value is ISyntaxNode or ISyntaxTree and not ISyntaxPart)
			target.Add("Kind", value.GetType().Name);
	}
	protected virtual string GetLabel(PropertyInfo property) => property.Name;
	protected virtual bool Include(PropertyInfo property, object container)
	{
		if (container is ISyntaxNode && property.Name is nameof(ISyntaxNode.Parent) or nameof(ISyntaxNode.Position))
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
	protected virtual int Compare(IDebugTreeObject target, IDebugTreeProperty left, IDebugTreeProperty right)
	{
		bool leftKind = left.Label is "Kind";
		bool rightKind = right.Label is "Kind";

		bool leftComplex = IsComplex(left.Value);
		bool rightComplex = IsComplex(right.Value);

		return
			Compare(leftKind, rightKind) ??
			Compare(leftComplex is false, rightComplex is false) ??
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
			IDebugTreeObject obj => obj.Properties.Count is 0,
			IDebugTreeList list => list.Elements.Count is 0,

			_ => false,
		};
	}

	protected virtual object? CreateValue(object? value)
	{
		if (IsList(value, out IEnumerable? enumerable))
		{
			DebugTreeList list = new();
			Populate(list, enumerable);

			return list;
		}

		if (IsObject(value))
		{
			DebugTreeObject obj = new();
			Populate(obj, value);

			return obj;
		}

		return value;
	}
	protected virtual bool IsList([NotNullWhen(true)] object? value, [NotNullWhen(true)] out IEnumerable? enumerable)
	{
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
}

public static class SyntaxTypeExtensions
{
	extension(Type type)
	{
		#region Methods
		public bool IsSyntaxNode() => type.IsAssignableTo(typeof(ISyntaxNode));
		public bool IsTriviaNode() => type.IsAssignableTo(typeof(ISyntaxTrivia));
		public bool IsTokenNode() => type.IsAssignableTo(typeof(ISyntaxToken));
		public bool IsSyntaxListNode() => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISyntaxList<>));
		public bool IsSeparatedSyntaxListNode() => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISyntaxList<,>));
		#endregion
	}

	extension(ParameterInfo parameter)
	{
		#region Methods
		public bool IsSyntaxNode() => parameter.ParameterType.IsSyntaxNode();
		public bool IsTriviaNode() => parameter.ParameterType.IsTriviaNode();
		public bool IsTokenNode() => parameter.ParameterType.IsTokenNode();
		public bool IsSyntaxListNode() => parameter.ParameterType.IsSyntaxListNode();
		public bool IsSeparatedSyntaxListNode() => parameter.ParameterType.IsSeparatedSyntaxListNode();
		#endregion
	}
}
