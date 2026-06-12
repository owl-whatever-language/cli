namespace OwlDomain.ParsingTools.Classification;

/// <summary>
///   Represents a list of classification kinds.
/// </summary>
public sealed class ClassificationList : IReadOnlyList<ClassificationKind>
{
	#region Fields
	private readonly IReadOnlyList<ClassificationKind> _values;
	#endregion

	#region Properties
	/// <inheritdoc/>
	public int Count => _values.Count;
	#endregion

	#region Indexers
	/// <inheritdoc/>
	public ClassificationKind this[int index] => _values[index];
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="ClassificationList"/> instance.</summary>
	public ClassificationList() => _values = [];

	/// <summary>Creates a new <see cref="ClassificationList"/> instance.</summary>
	/// <param name="kinds">The classification kinds to store in the list.</param>
	public ClassificationList(params IEnumerable<ClassificationKind> kinds)
	{
		_values = kinds.Distinct().ToArray();
	}
	/// <summary>Creates a new <see cref="ClassificationList"/> instance.</summary>
	/// <param name="kinds">The classification kinds to store in the list.</param>
	public ClassificationList(params ReadOnlySpan<ClassificationKind> kinds)
	{
		// Note(Nightowl): This can definitely be optimised;
		_values = kinds.ToArray().Distinct().ToArray();
	}
	#endregion

	#region Methods
	/// <inheritdoc/>
	public override string ToString() => Count is 0 ? "none" : string.Join(", ", _values);

	/// <inheritdoc/>
	public IEnumerator<ClassificationKind> GetEnumerator() => _values.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	#endregion
}