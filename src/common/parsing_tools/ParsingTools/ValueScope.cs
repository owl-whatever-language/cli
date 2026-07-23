namespace OwlDomain.ParsingTools;

public readonly ref struct ValueScope<T>(ref T storage, T oldValue) : IDisposable
{
	#region Fields
	private readonly ref T _storage = ref storage;
	private readonly T _oldValue = oldValue;
	#endregion

	#region Methods
	public void Dispose() => _storage = _oldValue;
	#endregion
}

public static class Value
{
	#region Functions
	public static ValueScope<T> Scope<T>(ref T storage, T newValue)
	{
		T oldValue = storage;
		storage = newValue;

		return new(ref storage, oldValue);
	}
	#endregion
}
