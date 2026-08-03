namespace OwlDomain.ParsingTools;

public static class EnumerableExtensions
{
	extension<T>(IEnumerable<T> enumerable)
	{
		#region Methods
		public T? IfSingleOrDefault(T? fallback = default)
		{
			if (enumerable is IList<T> { Count: var count } list)
			{
				return count switch
				{
					1 => list[0],
					_ => fallback,
				};
			}
			else
			{
				using IEnumerator<T> enumerator = enumerable.GetEnumerator();
				if (!enumerator.MoveNext())
					return fallback;

				T current = enumerator.Current;
				if (!enumerator.MoveNext())
					return current;
			}

			return fallback;
		}
		#endregion
	}
}
