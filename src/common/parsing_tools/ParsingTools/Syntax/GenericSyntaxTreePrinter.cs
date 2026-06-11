using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;

namespace OwlDomain.ParsingTools.Syntax;

/// <summary>
/// 	Represents a generic syntax tree printer that can be used for debugging different trees.
/// </summary>
[RequiresDynamicCode("This is used for debugging and uses reflection to account for all types of syntax trees.")]
public sealed class GenericSyntaxTreePrinter
{
	#region Properties
	/// <summary>The shared instance of the printer.</summary>
	public static GenericSyntaxTreePrinter Instance { get; } = new();
	#endregion

	#region Methods
	/// <summary>Prints the given syntax <paramref name="tree"/> to a <see langword="string"/>.</summary>
	/// <param name="tree">The syntax tree to print.</param>
	/// <returns>The <see langword="string"/> representation of the given <paramref name="tree"/>.</returns>
	public string Print(ISyntaxTree tree)
	{
		StringWriter stringWriter = new();
		IndentedTextWriter writer = new(stringWriter, " ");

		Print(writer, tree);

		return stringWriter.ToString();
	}

	/// <summary>Prints the given syntax <paramref name="tree"/> to the given <paramref name="writer"/>.</summary>
	/// <param name="writer">The writer to print the given <paramref name="tree"/> to.</param>
	/// <param name="tree">The syntax tree to print.</param>
	public void Print(IndentedTextWriter writer, ISyntaxTree tree)
	{
		writer.WriteLine($"Type: {tree.GetType().Name.RemoveSuffix("SyntaxTree")}");
		writer.WriteLine($"Source: {tree.Source.Path}");
		PrintValue(writer, "Document", tree.Document);
	}
	private void PrintValue(IndentedTextWriter writer, string label, object? value)
	{
		if (value is null)
			return;

		if (value is ITokenNode or ITriviaNode)
			PrintSimpleValue(writer, label, value);
		else if (value is IReadOnlyCollection<object?> list)
			PrintListValue(writer, label, list);
		else if (value is ISyntaxNode node)
			PrintStructureValue(writer, $"{label}: {node.Kind}", node);
		else
			PrintSimpleValue(writer, label, value);
	}
	private void PrintStructureValue(IndentedTextWriter writer, string label, object value)
	{
		Type type = value.GetType();
		List<PropertyInfo> properties = type
			.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
			.Where(p => IncludeProperty(value, p))
			.OrderByDescending(p =>
			{
				Debug.Assert(p.DeclaringType is not null);

				if (p.PropertyType.IsAssignableTo(typeof(ITokenNode)) || p.PropertyType.IsAssignableTo(typeof(ITriviaNode)))
					return true;

				if (p.PropertyType.IsAssignableTo(typeof(ISyntaxNode)))
					return false;

				if (p.PropertyType.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>)))
					return false;

				return true;
			})
			.ToList();

		if (properties.Count is 0)
			return;

		Span<string> order = [
			nameof(IConcreteSyntaxNode.IsFabricated),
			nameof(IConcreteSyntaxNode.Position),
			nameof(IConcreteSyntaxNode.FullPosition),
			"Symbol",
			"Type",
			"Function",
			"Value"
			];

		order.Reverse();

		foreach (string current in order)
		{
			PropertyInfo? property = properties.FirstOrDefault(p => p.Name.Equals(current, StringComparison.OrdinalIgnoreCase));
			if (property is not null)
			{
				properties.Remove(property);
				properties.Insert(0, property);
			}
		}

		writer.WriteLine($"{label}");

		writer.Indent += 2;

		foreach (PropertyInfo property in properties)
		{
			object? propertyValue = property.GetValue(value);
			PrintValue(writer, property.Name, propertyValue);
		}

		writer.Indent -= 2;
	}
	private bool IncludeProperty(object value, PropertyInfo property)
	{
		Type? declaringType = property.DeclaringType;
		Debug.Assert(declaringType is not null);

		if (property.PropertyType == typeof(SyntaxKind))
			return false;

		if (declaringType.IsAssignableTo(typeof(IAbstractSyntaxNode)) && property.Name == nameof(IAbstractSyntaxNode.Concrete))
			return false;

		if (declaringType.IsAssignableTo(typeof(ISemanticSyntaxNode)))
		{
			if (property.Name is nameof(ISemanticSyntaxNode.Abstract) or nameof(ISemanticSyntaxNode.Position))
				return false;
		}

		if (declaringType.IsAssignableTo(typeof(IConcreteSyntaxNode)))
		{
			if (property.Name == nameof(IConcreteSyntaxNode.IsFabricated))
				return property.GetValue(value) is true;
		}

		return true;
	}
	private void PrintListValue(IndentedTextWriter writer, string label, IReadOnlyCollection<object?> collection)
	{
		int maxLength = collection.Count.ToString("n0").Length;

		writer.WriteLine($"- {label}: ");
		writer.Indent += 2;

		int index = -1;

		foreach (object? value in collection)
		{
			index++;

			if (value is null)
				continue;

			PrintValue(writer, $"[{index.ToString("n0").PadLeft(maxLength)}]", value);
		}

		writer.Indent -= 2;
	}
	private void PrintSimpleValue(IndentedTextWriter writer, string label, object value)
	{
		string? text = value switch
		{
			ITokenNode token => token.Value is null ? $"\"{token.Lexeme?.Escape()}\"" : token.Lexeme?.Escape(),
			string str => $"\"{str.Escape()}\"",
			_ => value.ToString()
		};
		text ??= "";

		writer.WriteLine($"{label}: {text}");
	}
	#endregion
}

internal static class StringExtensions
{
	extension(string value)
	{
		public string RemoveSuffix(string suffix)
		{
			if (value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
				return value[..^suffix.Length];

			return value;
		}

		public string Escape()
		{
			StringBuilder builder = new();

			foreach (char current in value)
			{
				string newValue = current switch
				{
					'\r' => @"\r",
					'\n' => @"\n",
					'\\' => @"\\",

					_ => current.ToString()
				};

				builder.Append(newValue);
			}

			return builder.ToString();
		}
	}
}
