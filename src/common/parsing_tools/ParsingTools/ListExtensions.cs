namespace OwlDomain.ParsingTools;

public static class ListExtensions
{
	extension<T>(IReadOnlyList<T> list) where T : class
	{
		#region Methods
		public int IndexOf(T value)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == value)
					return i;
			}

			return -1;
		}
		#endregion
	}
	extension<T>(IReadOnlyList<T> list)
	{
		#region Methods
		public int FindIndex(Predicate<T> predicate)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (predicate.Invoke(list[i]))
					return i;
			}

			return -1;
		}

		public int FindLastIndex(Predicate<T> predicate)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (predicate.Invoke(list[i]))
					return i;
			}

			return -1;
		}
		#endregion
	}
}
