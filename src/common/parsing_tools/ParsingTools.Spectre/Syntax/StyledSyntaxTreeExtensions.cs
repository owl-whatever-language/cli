namespace OwlDomain.ParsingTools.Syntax;

public static class StyledSyntaxTreeExtensions
{
	extension(ISyntaxTree tree)
	{
		#region Methods
		public Rows StyledSource(ISyntaxNode root, IClassificationStyles styles, bool includeLineNumbers = true, Style? marginStyle = null)
		{
			return root.StyledSource(styles, includeLineNumbers, marginStyle);
		}
		public Panel StyledSourcePanel(ISyntaxNode root, IClassificationStyles styles, bool includeLineNumbers = true, Style? marginStyle = null)
		{
			return root.StyledSourcePanel(tree.Source, styles, includeLineNumbers, marginStyle);
		}
		#endregion
	}
}
