namespace OwlDomain.Owl.LSP;

public class WorkspaceSourceFile : ISourceFile
{
	#region Properties
	public string Path { get; }
	public string SimpleName { get; }

	public string Text
	{
		get;
		set
		{
			field = value;
			PositionTranslator = null;
		}
	}

	[AllowNull]
	public IPositionTranslator PositionTranslator
	{
		get => field ??= new PositionTranslator(Text);
		set;
	}
	#endregion

	#region Constructors
	public WorkspaceSourceFile(string path, string text)
	{
		Path = path;
		SimpleName = System.IO.Path.GetFileName(path);
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
