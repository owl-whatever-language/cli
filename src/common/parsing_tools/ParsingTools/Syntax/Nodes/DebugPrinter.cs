using System.CodeDom.Compiler;
using System.IO;

namespace OwlDomain.ParsingTools.Syntax.Nodes;

/// <summary>
/// 	A debug printer for printing the full lexeme of concrete syntax nodes.
/// </summary>
public static class DebugPrinter
{
	#region Functions
	/// <summary>Gets the full lexeme of the given syntax <paramref name="node"/>.</summary>
	/// <param name="node">The syntax node to print.</param>
	/// <param name="includeTrivia">Whether trivia should be included in the trivia node.</param>
	/// <returns>A <see langword="string"/> representation of the given syntax <paramref name="node"/>.</returns>
	/// <remarks>If trivia is excluded then some of the output might not make sense since tokens could look merged.</remarks>
	public static string ToString(IConcreteSyntaxNode node, bool includeTrivia = true)
	{
		StringWriter stringWriter = new();
		IndentedTextWriter writer = new(stringWriter);

		Write(writer, node, includeTrivia);
		return stringWriter.ToString();
	}

	private static void Write(IndentedTextWriter writer, IConcreteSyntaxNode node, bool includeTrivia)
	{
		if (node is ITokenNode token)
		{
			if (includeTrivia)
				Write(writer, token.LeadingTrivia);

			writer.Write(token.Lexeme);

			if (includeTrivia)
				Write(writer, token.TrailingTrivia);
		}
		else if (node is TriviaList triviaList)
			Write(writer, triviaList);
		else if (node is ITriviaNode trivia)
			Write(writer, trivia);
		else
		{
			foreach (IConcreteSyntaxNode child in node.GetChildren())
				Write(writer, child, includeTrivia);
		}
	}
	#endregion

	#region Trivia functions
	private static void Write(IndentedTextWriter writer, TriviaList list)
	{
		foreach (ITriviaNode trivia in list)
			Write(writer, trivia);
	}
	private static void Write(IndentedTextWriter writer, ITriviaNode trivia)
	{
		if (trivia.Value is IConcreteSyntaxNode node)
			Write(writer, node, true);
		else
			writer.Write(trivia.Lexeme);
	}
	#endregion
}
