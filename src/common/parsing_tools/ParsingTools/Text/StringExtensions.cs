namespace OwlDomain.ParsingTools.Text;

public static class StringExtensions
{
	extension(string? value)
	{
		#region Methods
		[return: NotNullIfNotNull(nameof(value))]
		public string? TryIntern()
		{
			if (value is null)
				return null;

			return string.IsInterned(value) ?? value;
		}

		[return: NotNullIfNotNull(nameof(value))]
		public string? EnsureInterned()
		{
			if (value is null)
				return null;

			string? interned = string.IsInterned(value);
			if (interned is null)
				ThrowHelper.ThrowArgumentException(nameof(value), $"Expected the string value ({value}) to already be interned.");

			return interned;
		}
		#endregion
	}
	extension(Guard)
	{
		public static void IsInterned(string value, [CallerArgumentExpression(nameof(value))] string name = "")
		{
			if (string.IsInterned(value) is null)
				ThrowHelper.ThrowArgumentException(name, $"Expected the string value ({value}) to already be interned.");
		}
	}
}
