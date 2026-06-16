using OwlDomain.ParsingTools.Generator.Syntax;

namespace OwlDomain.ParsingTools.Generator;

public static class DictionaryExtensions
{
	extension<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dictionary)
	{
		#region Methods
		public TValue? GetValueOrDefault(TKey key, TValue? fallback = default)
		{
			if (dictionary.TryGetValue(key, out TValue? value))
				return value;

			return fallback;
		}
		#endregion
	}
	extension<TValue>(IReadOnlyDictionary<string, TValue> dictionary)
	{
		#region Methods
		public TValue? GetValueOrDefault(Name key, TValue? fallback = default)
		{
			if (dictionary.TryGetValue(key.Original, out TValue? value))
				return value;

			return fallback;
		}
		#endregion
	}
}
