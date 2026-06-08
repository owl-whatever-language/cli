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
		ITokenNode? last = null;

		foreach (ITokenNode token in FlattenTokens(node))
		{
			if (last is not null)
				Write(writer, last.TrailingTrivia);

			if (includeEndTrivia && last == null)
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

	#region Helpers
	private static IEnumerable<ITokenNode> FlattenTokens(ISyntaxNode node)
	{
		List<ISyntaxNode> store = [];
		GetAllChildren(node, store);

		return store.OfType<ITokenNode>();
	}
	private static void GetAllChildren(ISyntaxNode node, List<ISyntaxNode> store)
	{
		if (node is ITriviaNode trivia)
		{
			if (trivia.Value is ISyntaxNode value)
				GetAllChildren(value, store);
		}
		else if (node is TriviaList triviaList)
		{
			foreach (ITriviaNode current in triviaList)
				GetAllChildren(current, store);
		}
		else if (node is ITokenNode token)
		{
			GetAllChildren(token.LeadingTrivia, store);
			store.Add(token);
			GetAllChildren(token.TrailingTrivia, store);
		}
		else
		{
			foreach (ISyntaxNode child in node.GetChildren())
				GetAllChildren(child, store);
		}
	}
	#endregion
}
