namespace OwlDomain.ParsingTools.CodeFixes;

/// <summary>
/// 	Represents information about a possible code fixing operation.
/// </summary>
public interface ICodeFix
{
	#region Properties
	/// <summary>The name for the code fix.</summary>
	string Name { get; }

	/// <summary>The description for what the code fix will do.</summary>
	string? Description { get; }

	/// <summary>The code changes that the code fix wants to make.</summary>
	IReadOnlyCollection<ICodeFixChange> Changes { get; }
	#endregion
}

/// <summary>
/// 	Contains various extensions related to code fixes.
/// </summary>
public static class CodeFixExtensions
{
	extension(IEnumerable<ICodeFixChange> changes)
	{
		#region Methods
		/// <summary>Groups the code fix changes by the source file that they are from.</summary>
		/// <returns>an enumerable of the grouped code fix changes.</returns>
		public IEnumerable<IGrouping<ISourceFile, ICodeFixChange>> GroupBySource() => changes.GroupBy(c => c.SourceFile);

		/// <summary>Orders the code fix changes by the start position of their replacement range.</summary>
		/// <returns>An enumerable of the code fix changes by their start position.</returns>
		/// <remarks>You likely want to group the changes by their source file first, before the ordering.</remarks>
		public IEnumerable<ICodeFixChange> OrderByStart() => changes.OrderBy(c => c.Range.Start);
		#endregion
	}
}

/// <summary>
/// 	Represents information about a possible code fixing operation.
/// </summary>
public sealed class CodeFix : ICodeFix
{
	#region Properties
	/// <inheritdoc/>
	public string Name { get; }

	/// <inheritdoc/>
	public string? Description { get; }

	/// <inheritdoc/>
	public IReadOnlyCollection<ICodeFixChange> Changes { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new code fix.</summary>
	/// <param name="name">The name of the code fix.</param>
	/// <param name="description">The description for what the code fix will do.</param>
	/// <param name="changes">The code changes that the fix would make.</param>
	/// <exception cref="ArgumentException">
	/// 	Thrown if either the <paramref name="name"/> is null, empty or only consists of white-space characters.
	/// 	Or if the <paramref name="changes"/> collection was empty.
	/// </exception>
	public CodeFix(string name, string? description, IReadOnlyCollection<ICodeFixChange> changes)
	{
		Guard.IsNotNullOrWhiteSpace(name);
		Guard.IsNotEmpty(changes);

		Name = name;
		Description = description;
		Changes = changes;
	}

	/// <summary>Creates a new code fix.</summary>
	/// <param name="name">The name of the code fix.</param>
	/// <param name="changes">The code changes that the fix would make.</param>
	/// <exception cref="ArgumentException">
	/// 	Thrown if either the <paramref name="name"/> is null, empty or only consists of white-space characters.
	/// 	Or if the <paramref name="changes"/> collection was empty.
	/// </exception>
	public CodeFix(string name, IReadOnlyCollection<ICodeFixChange> changes)
	{
		Guard.IsNotNullOrWhiteSpace(name);
		Guard.IsNotEmpty(changes);

		Name = name;
		Changes = changes;
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override string ToString() => Name;
	#endregion
}
