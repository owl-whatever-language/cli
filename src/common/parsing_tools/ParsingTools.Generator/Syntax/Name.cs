namespace OwlDomain.ParsingTools.Generator.Syntax;

public sealed class Name(string original) : IEquatable<Name>
{
	#region Properties
	public string Original { get; } = original;
	public IReadOnlyList<string> Parts => Original.Split(['_'], StringSplitOptions.RemoveEmptyEntries);
	public string PascalCase => ToPascalCase(Parts);
	public string CamelCase => ToCamelCase(Parts);
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
	private static string ToPascalCase(IEnumerable<string> parts) => string.Concat(parts.Select(ToPascal));
	private static string ToCamelCase(IReadOnlyList<string> parts)
	{
		if (parts.Count is 0)
			return "";

		if (parts.Count is 1)
			return ToCamel(parts[0]);

		string first = ToCamel(parts[0]);
		string pascal = ToPascalCase(parts.Skip(1));

		return first + pascal;
	}

	private static string ToPascal(string part)
	{
		if (part.Length is 0)
			return part;

		if (part.Length is 1)
			return part.ToUpper();

		char ch = char.ToUpper(part[0]);
		string remainder = part.Substring(1);

		return ch + remainder;
	}
	private static string ToCamel(string part)
	{
		if (part.Length is 0)
			return part;

		if (part.Length is 1)
			return part.ToLower();

		char ch = char.ToLower(part[0]);
		string remainder = part.Substring(1);

		return ch + remainder;
	}
	#endregion

	#region Operators
	public static implicit operator Name(string str) => new(str);
	public static bool operator ==(Name left, Name right) => left.Equals(right);
	public static bool operator !=(Name left, Name right) => left.Equals(right) is false;
	#endregion
}
