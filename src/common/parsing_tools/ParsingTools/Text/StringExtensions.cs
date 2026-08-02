namespace OwlDomain.ParsingTools.Text;

public static class StringExtensions
{
	extension(string? value)
	{
		#region Properties
		public string IndefiniteArticle
		{
			get
			{
				if (value is null || value.IsWhiteSpace())
					return "";

				char first = char.ToLower(value[0]);
				if (first is 'a' or 'e' or 'i' or 'o' or 'u')
					return "an";

				return "a";
			}
		}
		#endregion

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

		[return: NotNullIfNotNull(nameof(value))]
		public string? RemovePrefix(string suffix, bool allowMultiple = false)
		{
			if (value is null)
				return null;

			if (allowMultiple)
			{
				while (value.StartsWith(suffix))
					value = value[suffix.Length..];
			}
			else if (value.StartsWith(suffix))
				return value[suffix.Length..];

			return value;
		}

		[return: NotNullIfNotNull(nameof(value))]
		public string? RemoveSuffix(string suffix, bool allowMultiple = false)
		{
			if (value is null)
				return null;

			if (allowMultiple)
			{
				while (value.EndsWith(suffix))
					value = value[..^suffix.Length];
			}
			else if (value.EndsWith(suffix))
				return value[..^suffix.Length];

			return value;
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
