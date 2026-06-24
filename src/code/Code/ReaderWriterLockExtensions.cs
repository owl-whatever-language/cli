namespace OwlDomain.Owl.Code;

public readonly struct ReaderWriterReadLock(ReaderWriterLockSlim @lock) : IDisposable
{
	#region Methods
	public void Dispose() => @lock.ExitReadLock();
	#endregion
}

public readonly struct ReaderWriterWriteLock(ReaderWriterLockSlim @lock) : IDisposable
{
	#region Methods
	public void Dispose() => @lock.ExitWriteLock();
	#endregion
}

public readonly struct ReaderWriterUpgradeableReadLock(ReaderWriterLockSlim @lock) : IDisposable
{
	#region Methods
	public void Dispose() => @lock.ExitUpgradeableReadLock();
	#endregion
}

public static class ReaderWriterLockExtensions
{
	extension(ReaderWriterLockSlim @lock)
	{
		#region Methods
		public ReaderWriterReadLock ReadLock()
		{
			@lock.EnterReadLock();
			return new(@lock);
		}
		public ReaderWriterWriteLock WriteLock()
		{
			@lock.EnterWriteLock();
			return new(@lock);
		}
		public ReaderWriterUpgradeableReadLock UpgradeableReadLock()
		{
			@lock.EnterUpgradeableReadLock();
			return new(@lock);
		}
		#endregion
	}
}
