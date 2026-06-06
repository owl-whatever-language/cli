namespace OwlDomain.ParsingTools.Diagnostics;

/// <summary>
/// 	Represents a read-only collection of diagnostics.
/// </summary>
public interface IDiagnosticBag : IReadOnlyCollection<IDiagnostic>
{
	#region Properties
	/// <summary>Whether any errors are present.</summary>
	/// <remarks>This checks for <see cref="DiagnosticKind.Error"/>.</remarks>
	bool HasErrors { get; }
	#endregion
}

/// <summary>
/// 	Represents a mutable collection of diagnostics.
/// </summary>
public sealed class DiagnosticBag : IDiagnosticBag, ICollection<IDiagnostic>
{
	#region Fields
	private readonly List<IDiagnostic> _diagnostics = [];
	#endregion

	#region Properties
	/// <inheritdoc/>
	public int Count => _diagnostics.Count;

	/// <inheritdoc/>
	bool ICollection<IDiagnostic>.IsReadOnly => false;

	/// <inheritdoc/>
	public bool HasErrors => _diagnostics.Any(d => d.Kind == DiagnosticKind.Error);
	#endregion

	#region Methods
	/// <inheritdoc/>
	public void Add(IDiagnostic item) => _diagnostics.Add(item);

	/// <inheritdoc/>
	public bool Contains(IDiagnostic item) => _diagnostics.Contains(item);

	/// <inheritdoc/>
	public void CopyTo(IDiagnostic[] array, int arrayIndex) => _diagnostics.CopyTo(array, arrayIndex);

	/// <inheritdoc/>
	public bool Remove(IDiagnostic item) => _diagnostics.Remove(item);

	/// <inheritdoc/>
	public void Clear() => _diagnostics.Clear();

	/// <inheritdoc/>
	public IEnumerator<IDiagnostic> GetEnumerator() => _diagnostics.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	#endregion
}
