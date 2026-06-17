namespace OwlDomain.ParsingTools.Text;

public static class StringExtensions
{
	#region Methods
	[return: NotNullIfNotNull(nameof(value))]
	public static string? TryIntern(this string? value)
	{
		if (value is null)
			return null;

		return string.IsInterned(value) ?? value;
	}

	[return: NotNullIfNotNull(nameof(value))]
	public static string? EnsureInterned(this string? value)
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
