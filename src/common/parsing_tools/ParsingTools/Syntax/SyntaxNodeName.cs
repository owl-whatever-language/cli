namespace OwlDomain.ParsingTools.Syntax;

public readonly struct SyntaxNodeKind
{
	#region Properties
	public string? Kind { get; }
	public string Name { get; }
	public string? Group { get; }
	public string WithKind => Kind is null ? Name : Kind + "_" + Name;
	public string WithGroup => Group is null ? Name : Name + "_" + Group;
	public string FullName => GetFullName();
	#endregion

	#region Constructors
	public SyntaxNodeKind(string? kind, string name, string? group)
	{
		Kind = kind;
		Name = name;
		Group = group;
	}
	#endregion

	#region Methods
	private string GetFullName()
	{
		if (Kind is not null && Group is not null)
			return Kind + "_" + Name + "_" + Group;

		if (Kind is not null)
			return Kind + "_" + Name;

		if (Group is not null)
			return Name + "_" + Group;

		return Name;
	}
	public override string ToString() => FullName;
	#endregion
}
