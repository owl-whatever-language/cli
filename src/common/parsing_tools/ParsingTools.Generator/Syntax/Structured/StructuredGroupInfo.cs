namespace OwlDomain.ParsingTools.Generator.Syntax.Structured;

internal sealed class StructuredGroupInfo : BaseStructuredTreePart, IStructuredShadowedInfo<StructuredGroupInfo>
{
	#region Fields
	private Name? _nameWithTree;
	private IReadOnlyList<Name>? _names;
	#endregion

	#region Properties
	public Name Name { get; }
	public string Namespace { get; }
	public Name NameWithTree => _nameWithTree ??= new(Tree.Kind, Name);
	public IReadOnlyList<Name> Names => _names ??= [Name, NameWithTree];
	public StructuredInterfaceInfo Interface { get; }
	public IReadOnlyList<StructuredMemberInfo> Members { get; } = [];
	public List<StructuredNodeInfo> Nodes { get; } = [];
	public StructuredGroupInfo? Shadows { get; }
	public StructuredGroupInfo? ShadowedBy { get; private set; }
	public string Directory => $"{Tree.Directory}/{Name.Plural.Pascal}";
	public string Path => $"{Directory}/{Interface.Name}.g.cs";
	#endregion

	#region Constructors
	public StructuredGroupInfo(
		Name name,
		StructuredTreeInfo tree,
		string @namespace,
		StructuredInterfaceInfo @interface,
		IReadOnlyList<StructuredMemberInfo> members,
		StructuredGroupInfo? shadows) : base(tree)
	{
		Name = name;
		Namespace = @namespace;

		Interface = @interface;
		Members = members;

		Shadows = shadows;
		shadows?.ShadowedBy = this;
	}
	#endregion

	#region Methods
	public bool MatchesName(Name name)
	{
		foreach (Name current in Names.Reverse())
		{
			if (current == name)
				return true;
		}

		return false;
	}
	#endregion
}
