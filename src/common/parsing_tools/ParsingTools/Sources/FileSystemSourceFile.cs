using System.IO;

namespace OwlDomain.ParsingTools.Sources;

/// <summary>
/// 	Represents information about a source file that exists on the file system.
/// </summary>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class FileSystemSourceFile : ISourceFile
{
	#region Properties
	/// <inheritdoc/>
	public string SimpleName => FileInfo.Name;

	/// <inheritdoc/>
	public string Path => FileInfo.FullName;

	/// <summary>The system information about the source file.</summary>
	public FileInfo FileInfo { get; }

	public string Text { get; set; }
	#endregion

	#region Constructors
	/// <summary>Creates a new file system source file.</summary>
	/// <param name="path">The path to the source file.</param>
	public FileSystemSourceFile(string path)
	{
		FileInfo = new(path);
		Text = File.ReadAllText(path);
	}

	/// <summary>Creates a new file system source file.</summary>
	/// <param name="fileInfo">The system information about the source file.</param>
	public FileSystemSourceFile(FileInfo fileInfo)
	{
		FileInfo = fileInfo;
		Text = File.ReadAllText(fileInfo.FullName);
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
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

	#region Helpers
	private string DebuggerDisplay() => $"Source({SimpleName}): {Path}";
	#endregion
}
