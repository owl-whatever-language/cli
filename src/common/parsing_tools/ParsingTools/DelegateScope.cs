namespace OwlDomain.ParsingTools;

public readonly struct DelegateScope(Action callback) : IDisposable
{
	#region Fields
	private readonly Action _callback = callback;
	#endregion

	#region Methods
	public readonly void Dispose() => _callback.Invoke();
	#endregion
}
