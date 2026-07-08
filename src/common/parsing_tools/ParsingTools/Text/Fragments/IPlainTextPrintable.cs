using System.CodeDom.Compiler;
using System.IO;

namespace OwlDomain.ParsingTools.Text.Fragments;

public interface IPlainTextPrintable
{
	void WritePlainText(IndentedTextWriter writer);
}

public static class IPlainTextPrintableExtensions
{
	extension(IPlainTextPrintable printable)
	{
		#region Methods
		public void WritePlainText(StringWriter writer, string tab = IndentedTextWriter.DefaultTabString)
		{
			using (IndentedTextWriter indented = new(writer, tab))
				printable.WritePlainText(indented);
		}
		public void WritePlainText(StringBuilder builder, string tab = IndentedTextWriter.DefaultTabString)
		{
			using (StringWriter writer = new(builder))
			using (IndentedTextWriter indented = new(writer, tab))
				printable.WritePlainText(indented);
		}
		public string ToPlainText(string tab = IndentedTextWriter.DefaultTabString)
		{
			using (StringWriter writer = new())
			using (IndentedTextWriter indented = new(writer, tab))
			{
				printable.WritePlainText(indented);
				return writer.ToString();
			}
		}
		#endregion
	}
}
