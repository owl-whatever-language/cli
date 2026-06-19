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
}
