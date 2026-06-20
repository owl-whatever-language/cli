namespace OwlDomain.ParsingTools.Generator.Syntax;

internal readonly struct Name : IEquatable<Name>
{
	#region Fields
	private readonly string? _original;
	private readonly IReadOnlyList<string> _parts;
	#endregion

	#region Properties
	public static IReadOnlyCollection<string> Keywords { get; } =
	[
		"byte", "sbyte", "ushort", "short", "uint", "int", "ulong", "long",
		"float", "double", "decimal", "char",
		"char", "string",
		"bool", "true", "false",
		"base", "operator", "this", "if", "else", "while", "for", "return"
	];
	public string Original => _original ?? "";
	public IReadOnlyList<string> Parts => _parts ?? [];
	public string Camel => ToCamel(Parts);
	public string Pascal => ToPascal(Parts);
	public string Natural => ToNatural(Parts);
	public Name Plural => ToPlural(Parts);
	#endregion

	#region Constructors
	public Name(string original)
	{
		_original = original;
		_parts = original.Split(['_'], StringSplitOptions.RemoveEmptyEntries);
	}
	public Name(ReadOnlySpan<string> parts)
	{
		_parts = parts.ToArray();
		_original = string.Join("_", Parts);
	}
	public Name(params ReadOnlySpan<Name> names)
	{
		_parts = names.ToArray().SelectMany(n => n.Parts).ToArray();
		_original = string.Join("_", Parts);
	}
	#endregion

	#region Methods
	public bool Equals(Name other) => Original == other.Original;
	public override bool Equals(object obj)
	{
		if (obj is Name other)
			return Equals(other);

		return false;
	}

	public override int GetHashCode() => Original.GetHashCode();
	public override string ToString() => Original;
	#endregion

	#region Functions
	public static Name ToPlural(IReadOnlyList<string> fragments)
	{
		if (fragments.Count is 0)
			return string.Empty;

		string plural = ToPluralFragment(fragments.Last());

		if (fragments.Count is 1)
			return plural;

		string[] parts = new string[fragments.Count];
		for (int i = 0; i < fragments.Count - 1; i++)
			parts[i] = fragments[i];

		parts[parts.Length - 1] = plural;

		return string.Join("_", parts);
	}
	public static string ToPluralFragment(string name)
	{
		return ToSpecificPlural(name) ?? name;
	}
	private static string? ToSpecificPlural(string word)
	{
		return word switch
		{
			"body" => "bodies",

			_ => word + "s",
		};
	}

	public static string ToNatural(IReadOnlyList<string> fragments)
	{
		if (fragments.Count is 0)
			return string.Empty;

		string pascal = ToPascalFragment(fragments[0]);

		if (fragments.Count is 1)
			return pascal;

		IEnumerable<string> camel = fragments.Skip(1).Select(ToCamelFragment);
		return string.Join(" ", [pascal, .. camel]);
	}

	public static string ToCamel(IReadOnlyList<string> fragments)
	{
		if (fragments.Count is 0)
			return string.Empty;

		string camel = ToCamelFragment(fragments[0]);
		if (fragments.Count is 1)
			return camel;

		IEnumerable<string> pascal = fragments.Skip(1).Select(ToPascalFragment);
		return string.Concat([camel, .. pascal]);
	}
	public static string ToCamelFragment(string? fragment)
	{
		if (fragment is null || fragment.Length is 0)
			return string.Empty;

		return fragment.ToLower();
	}

	public static string ToPascal(IReadOnlyList<string> fragments)
	{
		if (fragments.Count is 0)
			return string.Empty;

		IEnumerable<string> pascal = fragments.Select(ToPascalFragment);
		return string.Concat(pascal);
	}
	public static string ToPascalFragment(string? fragment)
	{
		if (fragment is null || fragment.Length is 0)
			return string.Empty;

		fragment = fragment.ToLower();

		return char.ToUpper(fragment[0]) + fragment.Substring(1);
	}
	#endregion

	#region Operators
	public static implicit operator Name(string str) => new(str);
	public static bool operator ==(Name left, Name right) => left.Equals(right);
	public static bool operator !=(Name left, Name right) => left.Equals(right) is false;
	#endregion
}
