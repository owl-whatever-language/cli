namespace OwlDomain.Owl.Code.CodeAnalysis.Parsing;

public readonly struct EscapeSequence(char character, string value, string name)
{
	#region Properties
	public static IReadOnlyCollection<EscapeSequence> Known { get; } =
	[
		new('n', "\n", "Line break"),
		new('r', "\r", "Carriage return"),
		new('t', "\t", "Tab"),
		new('v', "\v", "Vertical tab"),
		new('f', "\f", "Form feed"),
		new('b', "\b", "Backspace"),
		new('a', "\a", "Carriage return"),
		new('e', "\e", "ANSI escape"),
		new('0', "\0", "Null character"),
		new('"', "\"", "Escaped quote mark"),
		new('\\', "\\", "Escaped backslash"),
	];
	public char Character { get; } = character;
	public string Value { get; } = value;
	public string Name { get; } = name;
	#endregion
}
