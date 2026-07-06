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
	public static MermaidControlFlowPrinter Instance => field ??= new(OwlStyling.Default);
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
			writer.WriteLine($"{branch.From.Id} --> {branch.ActualTo.Id}");
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
			writer.WriteLine($"\"|{branch.ActualTo.Id}");
		}
	}
	private bool IncludeBranch(IControlFlowBidirectionalBranch branch)
	{
		if (branch.From is IControlFlowMarkerBlock or IControlFlowConstructBlock)
			return false;

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

			if (expression.ConstructName is not null)
			{
				TextFragment name = new(expression.ConstructName, ClassificationKind.Keyword);
				TextFragmentLine nameLine = new(null, name);
				PrintHtmlLines(writer, [nameLine], center: true);
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

			for (int i = lines.Count - 1; i >= 0; i--)
			{
				if (lines[i].Count is 0 || lines[i].All(f => f.IsWhitespace || f.Classification == ClassificationKind.SinglelineComment))
					lines.RemoveAt(i);
			}

			lines.TrimLines().TrimSharedIndent().PrefixLineMargin();

			PrintHtmlLines(writer, lines);

			writer.WriteLine("\"]");
		}
	}
	#endregion

	#region Helpers
	private void PrintStartBlock(IndentedTextWriter writer, IControlFlowStartBlock block)
	{
		writer.Write($"{block.Id}(\"");
		TextFragment start = new("start", ClassificationKind.Keyword);
		PrintHtmlLines(writer, [new(null, start)], center: true);

		using (writer.NoIndent())
		{
			if (block.Graph is IDocumentControlFlowGraph document && document.Node.Tree is not null)
			{
				writer.Write($"{document.Node.Tree.Source.SimpleName}");
			}
			else if (block.Graph is IFunctionControlFlowGraph function)
			{
				TextFragmentLineCollection lines = function.Node.Signature.GetLines().TrimLines().PrefixLineMargin();
				PrintHtmlLines(writer, lines);
			}
		}

		writer.WriteLine("\")");
	}
	private void PrintEndBlock(IndentedTextWriter writer, IControlFlowEndBlock block)
	{
		writer.Write($"{block.Id}(((\"");
		TextFragment end = new("end", ClassificationKind.Keyword);
		PrintHtmlLines(writer, [new(null, end)], center: true);
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

		if (style == default)
		{
			writer.Write(encoded);
			return;
		}

		writer.Write("<span ");
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
