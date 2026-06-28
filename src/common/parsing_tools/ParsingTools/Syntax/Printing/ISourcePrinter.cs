namespace OwlDomain.ParsingTools.Syntax.Printing;

public interface ISourcePrinter
{
	#region Methods
	string GetDirect(ISyntaxNode node);
	string GetDebug(ISyntaxNode node);
	TextFragmentCollection GetDebugFragments(ISyntaxNode node);
	TextFragmentLineCollection GetLines(ISyntaxNode node);
	#endregion
}

public sealed class SourcePrinter : ISourcePrinter
{
	#region Properties
	public static SourcePrinter Instance { get; } = new();
	#endregion

	#region Methods
	public string GetDirect(ISyntaxNode node)
	{
		StringBuilder builder = new();

		foreach (ISyntaxPart part in node.ToParts())
			builder.Append(part.Lexeme);

		string text = builder.ToString();
		return text;
	}
	public string GetDebug(ISyntaxNode node)
	{
		StringBuilder builder = new();

		foreach (ISyntaxPart part in node.ToPartsWithoutOuterTrivia())
			builder.Append(part.Lexeme);

		string text = builder.ToString();
		return text;
	}
	public TextFragmentCollection GetDebugFragments(ISyntaxNode node)
	{
		TextFragmentCollection fragments = [];

		foreach (ISyntaxPart part in node.ToPartsWithoutOuterTrivia())
			fragments.TryAdd(part);

		return fragments;
	}
	public TextFragmentLineCollection GetLines(ISyntaxNode node)
	{
		TextFragmentLineCollection lines = [];
		TextFragmentLine? current = null;

		foreach (ISyntaxPart part in node.ToParts())
		{
			current ??= new(part.Position.Start.Line);

			if (part.Kind == SyntaxKind.LineBreak)
			{
				lines.Add(current);
				current = null;
			}
			else
				current.TryAdd(part);
		}

		if (current is not null)
			lines.Add(current);

		return lines;
	}
	#endregion
}

public static class ISourcePrinterExtensions
{
	extension(ISourcePrinter printer)
	{
		#region Methods
		public string GetDirect(ISyntaxTree tree) => printer.GetDirect(tree.Document);
		public string GetDebug(ISyntaxTree tree) => printer.GetDebug(tree.Document);
		public TextFragmentCollection GetDebugFragments(ISyntaxTree tree) => printer.GetDebugFragments(tree.Document);
		public TextFragmentLineCollection GetLines(ISyntaxTree tree) => printer.GetLines(tree.Document);
		#endregion
	}

	extension(ISyntaxNode node)
	{
		#region Methods
		public string GetDirectSource() => SourcePrinter.Instance.GetDirect(node);
		public string GetDebugSource() => SourcePrinter.Instance.GetDebug(node);
		public TextFragmentCollection GetDebugFragments() => SourcePrinter.Instance.GetDebugFragments(node);
		public TextFragmentLineCollection GetLines() => SourcePrinter.Instance.GetLines(node);
		#endregion
	}

	extension(ISyntaxTree tree)
	{
		#region Methods
		public string GetDirectSource() => SourcePrinter.Instance.GetDirect(tree);
		public string GetDebugSource() => SourcePrinter.Instance.GetDebug(tree);
		public TextFragmentCollection GetDebugFragments() => SourcePrinter.Instance.GetDebugFragments(tree);
		public TextFragmentLineCollection GetLines() => SourcePrinter.Instance.GetLines(tree);
		#endregion
	}
}
