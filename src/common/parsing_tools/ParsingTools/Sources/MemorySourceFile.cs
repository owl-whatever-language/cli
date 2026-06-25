namespace OwlDomain.ParsingTools.Sources;

/// <summary>
/// 	Represents information for a source file that only exists in memory.
/// </summary>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class MemorySourceFile : ISourceFile
{
	#region Properties
	/// <inheritdoc/>
	public string SimpleName { get; }

	/// <inheritdoc/>
	public string? Path => null;

	/// <summary>The source file text.</summary>
	public string Text { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new memory source file.</summary>
	/// <param name="name">The simple name for the source file.</param>
	/// <param name="text">The text in the source file.</param>
	/// <exception cref="ArgumentException">Thrown if the <paramref name="name"/> is null, empty or only consists of white-space characters.</exception>
	public MemorySourceFile(string name, string text)
	{
		Guard.IsNotNullOrWhiteSpace(name);

		SimpleName = name;
		Text = text;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public ITextParser CreateParser() => new StringTextParser(Text);
	public TextFragmentCollection GetFragments() => [new(SimpleName), new(": ", ClassificationKind.Punctuation), new("<memory>")];
	#endregion

	#region Helpers
	private string DebuggerDisplay() => $"Source({SimpleName}): <memory>";
	#endregion
}
