namespace OwlDomain.ParsingTools.CodeFixes;

/// <summary>
/// 	Represents the kind of the code fix change.
/// </summary>
public enum CodeFixChangeKind
{
	/// <summary>The code fix change will only add text.</summary>
	Addition,

	/// <summary>The code fix change will replace some of the text.</summary>
	Replacement,

	/// <summary>The code fix change will replace remove some of the text.</summary>
	Removal,
}

/// <summary>
/// 	Represents a code change that a code fix can make.
/// </summary>
public interface ICodeFixChange
{
	#region Properties
	/// <summary>The source file that the code fix will modify.</summary>
	ISourceFile SourceFile { get; }

	/// <summary>The position range in the source file that the code fix wants to modify.</summary>
	IndexedPositionRange Range { get; }

	/// <summary>The text that will replace the range.</summary>
	string Replacement { get; }

	/// <summary>The kind of the change.</summary>
	CodeFixChangeKind Kind { get; }

	/// <summary>A description for this change.</summary>
	string? Description { get; }
	#endregion
}

/// <summary>
/// 	Represents a code change that a code fix can make.
/// </summary>
public sealed class CodeFixChange : ICodeFixChange
{
	#region Properties
	/// <inheritdoc/>
	public required ISourceFile SourceFile { get; init; }

	/// <inheritdoc/>
	public required IndexedPositionRange Range { get; init; }

	/// <inheritdoc/>
	public string Replacement { get; init; } = string.Empty;

	/// <inheritdoc/>
	public CodeFixChangeKind Kind
	{
		get
		{
			if (Replacement.Length is 0)
				return CodeFixChangeKind.Removal;

			if (Range.Length is 0)
				return CodeFixChangeKind.Addition;

			return CodeFixChangeKind.Replacement;
		}
	}

	/// <inheritdoc/>
	public string? Description { get; init; }
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override string ToString()
	{
		return Kind switch
		{
			CodeFixChangeKind.Addition => $"Addition at {Range.Start}",
			CodeFixChangeKind.Removal => $"Removal at {Range}",
			CodeFixChangeKind.Replacement => $"Replacement at {Range}",

			_ => ThrowHelper.ThrowInvalidOperationException<string>($"Unknown code fix change kind ({Kind}).")
		};
	}
	#endregion
}
