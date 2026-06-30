namespace OwlDomain.Owl.Code.CodeAnalysis.Annotation;

public interface ICodeAnnotations
{
	#region Properties
	IEnumerable<ICodeAnnotation> All { get; }
	#endregion

	#region Methods
	void Add<T>(T annotation) where T : notnull, ICodeAnnotation;
	T Ensure<T>() where T : notnull, ICodeAnnotation, new();
	bool TryGet<T>([NotNullWhen(true)] out T? annotation) where T : notnull, ICodeAnnotation;
	#endregion
}

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}(), nq}}")]
public sealed class CodeAnnotations : ICodeAnnotations
{
	#region Fields
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly ReaderWriterLockSlim _lock = new();

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Dictionary<Type, ICodeAnnotation>? _annotations;
	public IEnumerable<ICodeAnnotation> All
	{
		get
		{
			using (_lock.ReadLock())
			{
				if (_annotations is null)
					yield break;

				foreach (ICodeAnnotation annotation in _annotations.Values)
					yield return annotation;
			}
		}
	}
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

	#region Helpers
	private string DebuggerDisplay()
	{
		int count;
		using (_lock.ReadLock())
			count = _annotations?.Count ?? 0;

		return $"Annotations: {count:n0}";
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

	private static void Collect<T>(List<T> target, ISyntaxNode current) where T : notnull, ICodeAnnotation
	{
		if (current is IAnnotatedSyntaxNode annotated && annotated.Annotations.TryGet(out T? annotation))
			target.Add(annotation);

		foreach (ISyntaxNode child in current.GetChildren())
			Collect(target, child);
	}

	extension(IAnnotatedSyntaxTree tree)
	{
		#region Methods
		public IReadOnlyCollection<T> CollectAnnotations<T>() where T : notnull, ICodeAnnotation
		{
			return tree.Document.CollectAnnotations<T>();
		}
		#endregion
	}
	extension(IAnnotatedSyntaxNode node)
	{
		#region Methods
		public IReadOnlyCollection<T> CollectAnnotations<T>() where T : notnull, ICodeAnnotation
		{
			List<T> target = [];
			Collect(target, node);

			return target;
		}
		#endregion
	}
	extension(IEnumerable<IAnnotatedSyntaxNode> nodes)
	{
		#region Methods
		public IReadOnlyCollection<T> CollectAnnotations<T>() where T : notnull, ICodeAnnotation
		{
			List<T> target = [];

			foreach (IAnnotatedSyntaxNode node in nodes)
				Collect(target, node);

			return target;
		}
		#endregion
	}
}
