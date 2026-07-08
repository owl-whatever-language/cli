using System.IO;
using System.Web;
using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Blocks;
using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Branches;
using OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Graphs;
using OwlDomain.Owl.Code.Styling;
using OwlDomain.ParsingTools.Syntax.Printing;

namespace OwlDomain.Owl.Code.CodeAnalysis.ControlFlow.Printing;

public sealed class MermaidControlFlowPrinter : IControlFlowPrinter<string>
{
	#region Properties
	private static bool Optimise => true;
	private static bool IncludeSourceReference => true;
	private static bool IncludeSymbolId => false;
	public static MermaidControlFlowPrinter Instance => field ??= new(OwlStyling.Default);
	private readonly ThreadLocal<Dictionary<ISymbol, int>> _symbolIds = new();
	public IClassificationStyling Styling { get; }
	#endregion

	#region Constructors
	public MermaidControlFlowPrinter(IClassificationStyling styling)
	{
		Styling = styling;
	}
	#endregion

	#region Methods
	public string Print(IControlFlowGraph graph)
	{
		_symbolIds.Value = [];

		using (StringWriter stringWriter = new())
		using (IndentedTextWriter writer = new(stringWriter, "  "))
		{
			writer.WriteLine("---");
			writer.WriteLine("title: Control flow graph");
			PrintConfig(writer);
			writer.WriteLine("---");
			writer.WriteLine("flowchart TB;");

			using (writer.Indented())
			{
				writer.WriteLine("%% Special blocks");
				PrintStartBlock(writer, graph.Start);
				PrintEndBlock(writer, graph.End);

				PrintUnconditionalBranches(writer, graph);
				PrintConditionalBranches(writer, graph);
				PrintExpressions(writer, graph);
				PrintStatements(writer, graph);
				PrintConstructs(writer, graph);

				if (IncludeSourceReference)
					PrintSourceReference(writer, graph);
			}

			return stringWriter.ToString();
		}
	}
	private void PrintConfig(IndentedTextWriter writer)
	{
		writer.WriteLine("config:");
		using (writer.Indented())
		{
			writer.WriteLine("themeVariables:");
			using (writer.Indented())
			{
				writer.WriteLine("darkMode: true");
				writer.WriteLine("fontFamily: 'Droid Sans Mono, monospace'");
				writer.WriteLine($"edgeLabelBackground: '{Styling.Background?.ToHtml}'");
			}
		}
	}
	private void PrintUnconditionalBranches(IndentedTextWriter writer, IControlFlowGraph graph)
	{
		IControlFlowBidirectionalBranch[] branches = graph.Branches
			.Where(b => b.HasCondition is false)
			.Where(IncludeBranch)
			.ToArray();

		if (branches.Length is 0)
			return;

		writer.WriteLine();
		writer.WriteLine("%% Unconditional branches");

		foreach (IControlFlowBidirectionalBranch branch in branches)
			writer.WriteLine($"{branch.From.Id} --> {GetTarget(branch).Id}");
	}

	private void PrintConditionalBranches(IndentedTextWriter writer, IControlFlowGraph graph)
	{
		IControlFlowBidirectionalBranch[] branches = graph.Branches
			.Where(b => b.HasCondition)
			.Where(IncludeBranch)
			.ToArray();

		if (branches.Length is 0)
			return;

		writer.WriteLine();
		writer.WriteLine("%% Conditional branches");

		foreach (IControlFlowBidirectionalBranch branch in branches)
		{
			writer.Write($"{branch.From.Id} -->|\"");

			string value = branch.IsNegated ? "false" : "true";
			TextFragment fragment = new(value, ClassificationKind.Boolean);
			PrintHtml(writer, fragment);
			writer.WriteLine($"\"|{GetTarget(branch).Id}");
		}
	}
	private bool IncludeBranch(IControlFlowBidirectionalBranch branch)
	{
		if (Optimise is false)
			return true;

		if (branch.From is IControlFlowMarkerBlock)
			return false;

		if (branch.From is IControlFlowConstructBlock construct)
			return IncludeConstruct(construct);

		return true;
	}

	private void PrintExpressions(IndentedTextWriter writer, IControlFlowGraph graph)
	{
		IControlFlowExpressionBlock[] expressions = graph.Blocks.OfType<IControlFlowExpressionBlock>().ToArray();

		if (expressions.Length is 0)
			return;

		writer.WriteLine();
		writer.WriteLine("%% Expressions");

		foreach (IControlFlowExpressionBlock expression in expressions)
		{
			bool isDecision = expression.Outgoing.Any(c => c.HasCondition);
			string start = isDecision ? "(" : "(";
			string end = isDecision ? ")" : ")";

			writer.Write($"{expression.Id}{start}\"");

			if (expression.ConstructName is not null && (expression.Expression.WillBranch is false))
			{
				int line = expression.Expression.Parent?.Position.Start.Line ?? expression.Expression.Position.Start.Line;
				TextFragment name = new(expression.ConstructName, ClassificationKind.Keyword);
				TextFragmentLine nameLine = new(line, name);
				TextFragmentLineCollection nameLines = [nameLine];

				PrintHtmlLines(writer, nameLines.PrefixLineMargin(), center: true);
			}

			TextFragmentLineCollection lines = expression.Expression.GetLines().TrimLines().TrimSharedIndent();
			PrintHtmlLines(writer, lines);

			writer.WriteLine($"\"{end}");
		}
	}
	private void PrintStatements(IndentedTextWriter writer, IControlFlowGraph graph)
	{
		IControlFlowStatementBlock[] statements = graph.Blocks.OfType<IControlFlowStatementBlock>().ToArray();

		if (statements.Length is 0)
			return;

		writer.WriteLine();
		writer.WriteLine("%% Statements");

		foreach (IControlFlowStatementBlock statement in statements)
		{
			writer.Write($"{statement.Id}[\"");

			TextFragmentLineCollection lines = [];
			foreach (IAnnotatedStatementSyntax current in statement.Statements)
				lines.AddRange(current.GetLines());

			lines.TrimCommented().TrimLines().TrimSharedIndent().PrefixLineMargin();

			PrintHtmlLines(writer, lines);

			writer.WriteLine("\"]");
		}
	}
	private void PrintConstructs(IndentedTextWriter writer, IControlFlowGraph graph)
	{
		IControlFlowConstructBlock[] constructs = graph.Blocks
			.OfType<IControlFlowConstructBlock>()
			.Where(IncludeConstruct)
			.ToArray();

		if (constructs.Length is 0)
			return;

		writer.WriteLine();
		writer.WriteLine("%% Constructs");

		foreach (IControlFlowConstructBlock construct in constructs)
		{
			Debug.Assert(construct.Expression is not null);
			writer.Write($"{construct.Id}(\"");

			TextFragment name = new(construct.Name, ClassificationKind.Keyword);
			TextFragmentLine nameLine = new(construct.Node.Position.Start.Line, name);
			TextFragmentLineCollection nameLines = [nameLine];
			PrintHtmlLines(writer, nameLines.PrefixLineMargin(), center: true);

			TextFragmentLineCollection lines = construct.Expression.GetLines().TrimLines().TrimSharedIndent();
			PrintHtmlLines(writer, lines);

			writer.WriteLine("\")");
		}
	}
	private bool IncludeConstruct(IControlFlowConstructBlock construct)
	{
		if (construct.Expression is null)
			return false;

		if (construct.Expression.WillBranch)
			return true;

		return false;
	}
	private void PrintSourceReference(IndentedTextWriter writer, IControlFlowGraph graph)
	{
		writer.WriteLine();
		writer.WriteLine("%% Source code reference");
		writer.Write("source_code_reference[\"");

		TextFragmentLine titleLine = new(null, [new("Source code reference", ClassificationKind.Keyword)]);
		TextFragmentLine fileLine = new(null, [new(graph.Node.GetTree().Source.SimpleName, ClassificationKind.File)]);
		PrintHtmlLines(writer, [titleLine, fileLine], center: true);
		writer.WriteLine();

		TextFragmentLineCollection lines = graph.Node.GetLines();

		HashSet<int> relevantLines = [];
		if (graph.Node is IConcreteFunctionDeclarationStatementSyntax function)
		{
			IndexedPositionRange range = function.Signature.Position;

			for (int line = range.Start.Line; line <= range.End.Line; line++)
				relevantLines.Add(line);
		}

		HashSet<int> irrelevantLines = [];
		foreach (IConcreteStatementSyntax statement in graph.Node.Flatten<IConcreteStatementSyntax>())
		{
			int start = statement.FullPosition.Start.Line;
			int end = statement.FullPosition.End.Line;
			IEnumerable<int> range = Enumerable.Range(start, end - start + 1);

			if (range.Any(relevantLines.Contains))
				continue;

			if (statement.IsExecutable && range.Any(irrelevantLines.Contains))
				continue;

			HashSet<int> target = statement.IsExecutable ? relevantLines : irrelevantLines;
			foreach (int line in range)
				target.Add(line);
		}

		foreach (int lineNumber in irrelevantLines)
		{
			Debug.Assert(relevantLines.Contains(lineNumber) is false);

			TextFragmentLine? line = lines.TryGetLineAt(lineNumber);
			if (line is not null)
				lines.Remove(line);
		}

		lines
			.TrimCommented()
			.TrimLines();

		lines
			.TrimSharedIndent()
			.PrefixLineMargin();

		PrintHtmlLines(writer, lines);

		writer.WriteLine("\"]");
	}
	#endregion

	#region Helpers
	private IControlFlowBlock GetTarget(IControlFlowOutgoingBranch branch)
	{
		if (Optimise is false)
			return branch.To;

		IControlFlowBlock to = branch.To;
		while (true)
		{
			if (to is IControlFlowMarkerBlock marker)
			{
				//Debug.Assert(marker.Outgoing.Count is 1);
				if (marker.Outgoing.Count is 1)
				{
					to = marker.Outgoing[0].To;
					continue;
				}
			}
			else if (to is IControlFlowConstructBlock construct && (IncludeConstruct(construct) is false))
			{
				if (construct.Outgoing.Count is 1)
				{
					to = construct.Outgoing[0].To;
					continue;
				}
			}

			break;
		}

		return to;
	}

	private void PrintStartBlock(IndentedTextWriter writer, IControlFlowStartBlock block)
	{
		writer.Write($"{block.Id}(\"");
		TextFragmentLine start = new(null, [new("start", ClassificationKind.Keyword)]);
		TextFragmentLineCollection startLines = [start];

		if (block.Graph is IDocumentControlFlowGraph document && document.Node.Tree is not null)
		{
			TextFragmentLine fileLine = new(null, [new(document.Node.Tree.Source.SimpleName, ClassificationKind.File)]);
			startLines.Add(fileLine);
		}

		PrintHtmlLines(writer, startLines, center: true);

		if (block.Graph is IFunctionControlFlowGraph function)
		{
			TextFragmentLineCollection lines = function.Node.Signature.GetLines().TrimLines().PrefixLineMargin();
			PrintHtmlLines(writer, lines);
		}

		writer.WriteLine("\")");
	}
	private void PrintEndBlock(IndentedTextWriter writer, IControlFlowEndBlock block)
	{
		writer.Write($"{block.Id}(((\"");
		TextFragment end = new("end", ClassificationKind.Keyword);
		PrintHtmlLines(writer, [new TextFragmentLine(null, end)], center: true);
		writer.WriteLine("\")))");
	}

	private void PrintHtmlLines(IndentedTextWriter writer, TextFragmentLineCollection lines, bool center = false)
	{
		using (writer.NoIndent())
		{
			PrintHtmlStart(writer, center);

			for (int i = 0; i < lines.Count; i++)
			{
				TextFragmentLine line = lines[i];

				foreach (TextFragment fragment in line)
					PrintHtml(writer, fragment);

				if (i + 1 < lines.Count)
					writer.WriteLine();
			}
			PrintHtmlEnd(writer);
		}
	}
	private void PrintHtmlStart(IndentedTextWriter writer, bool center)
	{
		string align = center ? "center" : "left";
		writer.Write($"<div style=\"white-space:pre;text-align:{align};tab-size:3\">");
	}
	private void PrintHtmlEnd(IndentedTextWriter writer)
	{
		writer.Write("</div>");
	}
	private void PrintHtml(IndentedTextWriter writer, TextFragment fragment)
	{
		StyleInfo style = Styling.Get(fragment.Classification);

		// Note(Nightowl): Because mermaid doesn't like semicolons;
		string[] parts = fragment.Text.Split(';');
		for (int i = 0; i < parts.Length; i++)
			parts[i] = HttpUtility.HtmlEncode(parts[i]);
		string encoded = string.Join("&#x3b;", parts);

		if (encoded is "\t")
			encoded = "   "; // Note(Nightowl): Yet another thing that doesn't actually respect tab size, even though it's explicitly set;

		if (style == default)
		{
			writer.Write(encoded);
			return;
		}

		writer.Write("<span ");
		if (IncludeSymbolId)
		{
			ISymbol? symbol = fragment.Symbol;
			if (symbol is not null)
			{
				Debug.Assert(_symbolIds.Value is not null);
				if (_symbolIds.Value.TryGetValue(symbol, out int id) is false)
				{
					id = _symbolIds.Value.Count + 1;
					_symbolIds.Value.Add(symbol, id);
				}
				writer.Write($"data-symbol-id=\"{id}\" ");
			}
		}

		writer.Write(style.ToHtmlStyle);
		writer.Write(">");
		writer.Write(encoded);
		writer.Write("</span>");
	}
	#endregion
}

public static class MermaidControlFlowPrinterExtensions
{
	extension(IControlFlowGraph graph)
	{
		#region Methods
		public string ToMermaid() => MermaidControlFlowPrinter.Instance.Print(graph);
		#endregion
	}
}
