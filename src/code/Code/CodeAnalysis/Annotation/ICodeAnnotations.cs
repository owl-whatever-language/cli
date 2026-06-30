namespace OwlDomain.Owl.Code.CodeAnalysis.Annotation;

public interface ICodeAnnotations
{
	#region Methods
	void Add<T>(T annotation) where T : notnull, ICodeAnnotation;
	T Ensure<T>() where T : notnull, ICodeAnnotation, new();
	bool TryGet<T>([NotNullWhen(true)] out T? annotation) where T : notnull, ICodeAnnotation;
	#endregion
}

public sealed class CodeAnnotations : ICodeAnnotations
{
	#region Fields
	private readonly ReaderWriterLockSlim _lock = new();
	private Dictionary<Type, ICodeAnnotation>? _annotations;
	#endregion

	#region Methods
	public void Add<T>(T annotation) where T : notnull, ICodeAnnotation
	{
		using (_lock.WriteLock())
			AddNoLock(annotation);
	}
	public T Ensure<T>() where T : notnull, ICodeAnnotation, new()
	{
		using (_lock.ReadLock())
		{
			if (TryGetNoLock(out T? annotation))
				return annotation;
		}

		using (_lock.UpgradeableReadLock())
		{
			if (TryGetNoLock(out T? annotation))
				return annotation;

			using (_lock.WriteLock())
			{
				annotation = new();
				AddNoLock(annotation);

				return annotation;
			}
		}
	}
	public bool TryGet<T>([NotNullWhen(true)] out T? annotation) where T : notnull, ICodeAnnotation
	{
		using (_lock.ReadLock())
			return TryGetNoLock(out annotation);
	}
	private bool TryGetNoLock<T>([NotNullWhen(true)] out T? annotation) where T : notnull, ICodeAnnotation
	{
		if (_annotations is null)
		{
			annotation = default;
			return false;
		}

		if (_annotations.TryGetValue(typeof(T), out ICodeAnnotation? untyped))
		{
			annotation = (T)untyped;
			return true;
		}

		annotation = default;
		return false;
	}
	private void AddNoLock<T>(T annotation) where T : notnull, ICodeAnnotation
	{
		_annotations ??= [];
		_annotations.Add(typeof(T), annotation);
	}
	#endregion
}

public static class ICodeAnnotationsExtensions
{
	extension(ICodeAnnotations annotations)
	{
		#region Methods
		public T? TryGet<T>() where T : notnull, ICodeAnnotation
		{
			if (annotations.TryGet(out T? annotation))
				return annotation;

			return default;
		}
		public T Get<T>() where T : notnull, ICodeAnnotation
		{
			if (annotations.TryGet(out T? annotation) is false)
				ThrowHelper.ThrowInvalidOperationException($"No annotation of the type '{typeof(T)}' could be found.");

			return annotation;
		}
		public void Get<T>(out T annotation) where T : notnull, ICodeAnnotation
		{
			annotation = annotations.Get<T>();
		}
		#endregion
	}
}
