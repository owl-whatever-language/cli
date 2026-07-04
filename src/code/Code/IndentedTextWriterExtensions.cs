using System.Web;

namespace OwlDomain.Owl.Code;

public static class IndentedTextWriterExtensions
{
	#region Nested types
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
		#endregion
	}
}
