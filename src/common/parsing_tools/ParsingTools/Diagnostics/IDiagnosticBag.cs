namespace OwlDomain.ParsingTools.Diagnostics;

/// <summary>
/// 	Represents a read-only collection of diagnostics.
/// </summary>
public interface IDiagnosticBag : IReadOnlyCollection<IDiagnostic>
{
}

/// <summary>
/// 	Represents a mutable collection of diagnostics.
/// </summary>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
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

	#region Helpers
	private string DebuggerDisplay() => $"{nameof(DiagnosticBag)} {{ Count = ({Count:n0}), Errors = ({this.ErrorCount:n0}) }}";
	#endregion
}

public static class IDiagnosticBagExtensions
{
	extension(IDiagnosticBag bag)
	{
		#region Properties
		public bool HasErrors => bag.Any(d => d.Kind >= DiagnosticKind.Error);
		public int ErrorCount => bag.Count(d => d.Kind >= DiagnosticKind.Error);
		public int WarningCount => bag.Count(d => d.Kind >= DiagnosticKind.Warning && d.Kind < DiagnosticKind.Error);
		#endregion
	}
}