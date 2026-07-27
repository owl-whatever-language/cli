namespace OwlDomain.Owl.LSP;

public class WorkspaceSourceFile : ISourceFile
{
	#region Properties
	public string Path { get; }
	public string SimpleName { get; }
	public string Text { get; set; }
	#endregion

	#region Constructors
	public WorkspaceSourceFile(Uri uri, string text)
	{
		Path = uri.AbsolutePath;
		SimpleName = System.IO.Path.GetFileName(uri.AbsolutePath);
		Text = text;
	}
	#endregion

	#region Methods
	public ITextParser CreateParser() => new StringTextParser(Text);
	public TextFragmentCollection GetDebugText()
	{
		return
		[
			new(SimpleName),
			new(":", ClassificationKind.Punctuation),
			TextFragment.Space,
			new(Path)
		];
	}
	#endregion
}
