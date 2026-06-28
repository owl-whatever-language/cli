namespace OwlDomain.ParsingTools.Syntax;

public static class StyledSyntaxTreeExtensions
{
	extension(ISyntaxTree tree)
	{
		#region Methods
		public Rows StyledSource(IClassificationStyles styles, bool includeLineNumbers = true, Style? marginStyle = null)
		{
			return tree.Document.StyledSource(styles, includeLineNumbers, marginStyle);
		}
		public Panel StyledSourcePanel(IClassificationStyles styles, bool includeLineNumbers = true, Style? marginStyle = null)
		{
			return tree.Document.StyledSourcePanel(tree.Source, styles, includeLineNumbers, marginStyle);
		}
		#endregion
	}
}
