using System.CodeDom.Compiler;
using System.IO;

namespace OwlDomain.ParsingTools.Syntax;

/// <summary>
/// 	A debug printer for printing the full lexeme of concrete syntax nodes.
/// </summary>
public static class DebugPrinter
{
	#region Functions
	/// <summary>Gets the full lexeme of the given syntax <paramref name="node"/>.</summary>
	/// <param name="node">The syntax node to print.</param>
	/// <param name="includeEndTrivia">Whether the very first and very last trivia should be printed.</param>
	/// <returns>A <see langword="string"/> representation of the given syntax <paramref name="node"/>.</returns>
	/// <remarks>Not all trivia can be excluded as some is significant when printing to not make it look like the tokens are merged.</remarks>
	public static string ToString(IConcreteSyntaxNode node, bool includeEndTrivia = false)
	{
		StringWriter stringWriter = new();
		IndentedTextWriter writer = new(stringWriter);

		Write(writer, node, includeEndTrivia);
		return stringWriter.ToString();
	}

	private static void Write(IndentedTextWriter writer, IConcreteSyntaxNode node, bool includeEndTrivia)
	{
		IConcreteSyntaxToken? last = null;

		foreach (IConcreteSyntaxToken token in node.Flatten())
		{
			if (last is not null)
				Write(writer, last.TrailingTrivia);

			if (includeEndTrivia || last != null)
				Write(writer, token.LeadingTrivia);

			writer.Write(token.Lexeme);

			last = token;
		}

		if (includeEndTrivia && last is not null)
			Write(writer, last.TrailingTrivia);
	}
	#endregion

	#region Trivia functions
	private static void Write(IndentedTextWriter writer, TriviaList list)
	{
		foreach (ITriviaNode trivia in list)
			writer.Write(trivia.Lexeme);
	}
	#endregion
}
