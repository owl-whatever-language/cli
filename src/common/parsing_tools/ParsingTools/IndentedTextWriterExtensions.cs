using System.CodeDom.Compiler;
using System.Web;

namespace OwlDomain.ParsingTools;

public static class IndentedTextWriterExtensions
{
	#region Nested types
	public readonly struct MarkdownCodeScope(IndentedTextWriter writer) : IDisposable
	{
		#region Methods
		public void Dispose() => writer.WriteLine("```");
		#endregion
	}
	public readonly struct MarkdownSectionScope(IndentedTextWriter writer) : IDisposable
	{
		#region Methods
		public void Dispose() => writer.WriteLine();
		#endregion
	}

	public readonly struct IndentScope(IndentedTextWriter writer) : IDisposable
	{
		public void Dispose() => writer.Indent--;
	}
	public readonly struct SpecificIndentScope(IndentedTextWriter writer, int oldIndent) : IDisposable
	{
		public void Dispose() => writer.Indent = oldIndent;
	}
	#endregion

	extension(IndentedTextWriter writer)
	{
		#region Methods
		public IndentScope Indented()
		{
			writer.Indent++;
			return new(writer);
		}
		public SpecificIndentScope NoIndent() => Indented(writer, 0);
		public SpecificIndentScope Indented(int newIndent)
		{
			int old = writer.Indent;
			writer.Indent = newIndent;

			return new(writer, old);
		}
		public void WriteHtmlEscaped(string value)
		{
			value = HttpUtility.HtmlEncode(value);
			writer.Write(value);
		}
		public MarkdownCodeScope MarkdownCode(string language)
		{
			writer.Write("```");
			writer.WriteLine(language);

			return new(writer);
		}
		public MarkdownSectionScope MarkdownSection(int level, string header)
		{
			Guard.IsGreaterThan(level, 0);

			for (int i = 0; i < level; i++)
				writer.Write('#');

			writer.Write(' ');
			writer.WriteLine(header);

			return new(writer);
		}
		#endregion
	}
}
