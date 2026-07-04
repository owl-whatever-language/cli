using System.IO;
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
			writer.WriteLine("darkMode: true");
			writer.WriteLine("fontFamily: monospace %% This should work but it doesn't?");
			writer.WriteLine("---");
			writer.WriteLine("flowchart TB;");

			using (writer.Indented())
			{
				writer.WriteLine("%% Special blocks");
				writer.WriteLine($"{graph.Start.Id}[\"`Start`\"]");
				writer.WriteLine($"{graph.End.Id}[End]");

				IControlFlowStatementBlock[] statementBlocks = graph.Blocks.OfType<IControlFlowStatementBlock>().ToArray();
				if (statementBlocks.Any())
				{
					writer.WriteLine();
					writer.WriteLine("%% Statement blocks");
					foreach (IControlFlowStatementBlock block in statementBlocks)
						PrintBlock(writer, block);
				}

				IControlFlowExpressionBlock[] expressionBlocks = graph.Blocks.OfType<IControlFlowExpressionBlock>().ToArray();
				if (expressionBlocks.Any())
				{
					writer.WriteLine();
					writer.WriteLine("%% Expression blocks");
					foreach (IControlFlowExpressionBlock block in expressionBlocks)
						PrintBlock(writer, block);
				}

				writer.WriteLine();
				writer.WriteLine("%% Branches");
				foreach (IControlFlowBidirectionalBranch branch in graph.Branches)
					writer.WriteLine($"{branch.From.Id} --> {branch.To.Id}");
			}

			return stringWriter.ToString();
		}
	}
	#endregion

	#region Helpers
	private void PrintBlock(IndentedTextWriter writer, IControlFlowBlock block)
	{
		writer.Write($"{block.Id}[\"");

		if (block is IControlFlowStatementBlock statement)
			PrintStatementBlock(writer, statement);
		else if (block is IControlFlowExpressionBlock expression)
			PrintExpressionBlock(writer, expression);
		else
			ThrowHelper.ThrowInvalidOperationException($"Unknown control flow block type {block.GetType().Name}.");

		writer.WriteLine("\"]");
	}
	private void PrintStatementBlock(IndentedTextWriter writer, IControlFlowStatementBlock block)
	{
		TextFragmentLineCollection lines = [];

		IReadOnlyList<IAnnotatedStatementSyntax> statements = block.Statements;
		while (statements.Count is 1 && statements[0] is IAnnotatedBlockStatementSyntax blockStatement && blockStatement.Statements.Any())
			statements = blockStatement.Statements;

		foreach (IAnnotatedStatementSyntax statement in statements)
		{
			TextFragmentLineCollection collection = statement.GetLines();
			lines.AddRange(collection);
		}

		lines.TrimLines().PrefixLineMargin();

		PrintHtmlLines(writer, lines);
	}
	private void PrintExpressionBlock(IndentedTextWriter writer, IControlFlowExpressionBlock block)
	{
		TextFragmentLineCollection lines = block.Expression.GetLines().TrimLines();
		PrintHtmlLines(writer, lines);
	}
	private void PrintHtmlLines(IndentedTextWriter writer, TextFragmentLineCollection lines)
	{
		using (writer.NoIndent())
		{
			PrintHtmlStart(writer);

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

	private void PrintHtmlStart(IndentedTextWriter writer)
	{
		writer.Write("<div style=\"white-space:pre;text-align:left;tab-size:3;font-family:monospace\">");
	}
	private void PrintHtmlEnd(IndentedTextWriter writer)
	{
		writer.Write("</div>");
	}
	private void PrintHtml(IndentedTextWriter writer, TextFragment fragment)
	{
		StyleInfo style = Styling.Get(fragment.Classification);

		if (style == default)
		{
			writer.WriteHtmlEscaped(fragment.Text);
			return;
		}

		writer.Write("<span ");
		writer.Write(style.ToHtmlStyle);
		writer.Write(">");
		writer.WriteHtmlEscaped(fragment.Text);
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
