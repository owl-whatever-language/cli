using OwlDomain.Owl.Code.CodeAnalysis.Syntax;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.FunctionBodies;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Nodes;
using OwlDomain.Owl.Code.CodeAnalysis.Syntax.Concrete.Statements;
using Spectre.Console.Rendering;

namespace OwlDomain.Owl.CLI.Diagnostics;

public static class DiagnosticExplainer
{
	#region Functions
	public static IRenderable? Explain(IAnalysisContext context, params IReadOnlyCollection<IStageResult> results)
	{
		List<IRenderable> renderable = [];

		IDiagnostic[] diagnostics = results.GetAllDiagnostics().ToArray();
		HashSet<IDiagnostic> seen = [];

		foreach (IGrouping<ISourceFile?, IDiagnostic> group in diagnostics.GroupBy(d => d.Source))
		{
			if (group.Key is null)
				continue;

			if (context.TryGet(group.Key, out ISyntaxTreeBundle? bundle) is false || bundle.LeastDetailed is null)
				continue;

			foreach (IDiagnostic diagnostic in group)
			{
				DiagnosticSourceDisplay display = new(bundle.LeastDetailed, OwlStyling.Default);
				HashSet<ISyntaxNode> nodes = [];

				seen.Add(diagnostic);
				display.Add(diagnostic);

				foreach (ISyntaxNode node in diagnostic.RelevantNodes)
				{
					if (node.GetTree().Source == group.Key)
					{
						foreach (ISyntaxNode current in node.GetChain())
							nodes.Add(current);
					}
				}

				foreach (int line in GetRelevantLines(nodes))
					display.MarkRelevant(line, shouldDim: true);

				display.ApplyTypicalTransformations();

				TextFragmentLineCollection result = display
					.GetLines()
					.TrimLastLines()
					.PrefixLineMargin();

				display.ApplyDim();

				Rows groupedOutput = new([
					new Rule(group.Key.SimpleName).LeftJustified(),
					new Padder(result.Style(OwlStyling.Default)),
					new Rule()
				]);

				renderable.Add(groupedOutput);
			}
		}

		if (renderable.Count is 0)
			return null;

		return new Rows(renderable);
	}
	#endregion

	#region Helpers
	static IReadOnlyCollection<int> GetRelevantLines(IEnumerable<ISyntaxNode> nodes)
	{
		HashSet<int> lines = [];

		void Add(params ReadOnlySpan<ISyntaxNode> nodes)
		{
			foreach (ISyntaxNode node in nodes)
			{
				int start = node.Position.Start.Line;
				int end = node.Position.End.Line;

				for (int i = start; i <= end; i++)
					lines.Add(i);
			}
		}

		foreach (ISyntaxNode node in nodes)
		{
			if (node is IConcreteBlockStatementSyntax block)
				Add(block.Start, block.End);
			else if (node is IConcreteIfStatementSyntax @if)
				Add(@if.Keyword, @if.Start, @if.Condition, @if.End); // Note(Nightowl): The conditions might not strictly be necessary;
			else if (node is IConcreteIfElseStatementSyntax elseIf)
				Add(elseIf.Keyword, elseIf.Start, elseIf.Condition, elseIf.End, elseIf.Else);
			else if (node is IConcreteWhileStatementSyntax @while)
				Add(@while.Keyword, @while.Start, @while.Condition, @while.End);
			else if (node is IConcreteFunctionDeclarationSignatureSyntax signature)
				Add(signature);
			else if (node is IConcreteFunctionDeclarationStatementSyntax function)
				Add(function.Signature);
			else if (node is IConcreteShortFunctionBodySyntax shortFunction)
				Add(shortFunction.Arrow); // Note(Nightowl): If a short function body is the parent then the diagnostic would've been about the expression;
		}

		return lines;
	}
	#endregion
}
