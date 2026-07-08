namespace OwlDomain.Owl.Code.CodeAnalysis.Parsing;

public enum NumberBaseKind
{
	Unknown = 0,
	Binary = 2,
	Decimal = 10,
	Hexadecimal = 16,
}

public readonly struct NumberBase
{
	#region Known
	public static IReadOnlyCollection<NumberBase> Known { get; } = [Binary, Decimal, Hexadecimal];
	public static IReadOnlyCollection<NumberBase> WithSpecifier { get; } = Known.Where(b => b.Specifier is not null).ToArray();
	public static NumberBase Binary { get; } = new(NumberBaseKind.Binary, "0b", "0 or 1", "01");
	public static NumberBase Decimal { get; } = new(NumberBaseKind.Decimal, specifier: null, "0-9", "0123456789");
	public static NumberBase Hexadecimal { get; } = new(NumberBaseKind.Hexadecimal, "0x", "0-9 and a-f case-insensitive", "0123456789abcdefABCDEF");
	#endregion

	#region Properties
	public NumberBaseKind Kind { get; }
	public string Name => Kind.ToString();
	public int Base => (int)Kind;
	public string? Specifier { get; }
	public string CharacterSetDisplay { get; }
	public IReadOnlySet<char> CharacterSet { get; }
	#endregion

	#region Constructors
	public NumberBase(NumberBaseKind kind, string? specifier, string characterSetDisplay, string characterSet)
	{
		Kind = kind;
		Specifier = specifier;
		CharacterSetDisplay = characterSetDisplay;
		CharacterSet = characterSet.ToHashSet();
	}
	#endregion

	#region Methods
	public bool IsValid(string text) => text.All(CharacterSet.Contains);
	#endregion
}
