namespace OwlDomain.ParsingTools.Generator.Syntax.Structured;

internal class StructuredNodeInfo : BaseStructuredGroupPart, IStructuredShadowedInfo<StructuredNodeInfo>
{
	#region Fields
	private Name? _nameWithGroup;
	private Name? _nameWithTree;
	private IReadOnlyList<Name>? _names;
	#endregion

	#region Properties
	public Name Name { get; }
	public string Namespace { get; }
	public Name NameWithGroup => _nameWithGroup ??= new(Name, Group?.Name ?? default);
	public Name NameWithTree => _nameWithTree ??= new(Tree.Kind, NameWithGroup);
	public IReadOnlyList<Name> Names => _names ??= [Name, NameWithGroup, NameWithTree];
	public StructuredInterfaceInfo Interface { get; }
	public StructuredClassInfo Class { get; }
	public IReadOnlyList<StructuredMemberInfo> Members { get; }
	public StructuredNodeInfo? Shadows { get; }
	public StructuredNodeInfo? ShadowedBy { get; private set; }
	public string Directory => Group?.Directory ?? Tree.Directory + "/Nodes";
	public string Path => $"{Directory}/{Class.Name}.g.cs";
	public IEnumerable<StructuredMemberInfo> InterfaceMembers => GetInterfaceMembers();
	public IEnumerable<StructuredMemberInfo> ClassMembers => GetClassMembers();
	#endregion

	#region Constructors
	public StructuredNodeInfo(
		Name name,
		StructuredTreeInfo tree,
		StructuredGroupInfo? group,
		string @namespace,
		StructuredInterfaceInfo @interface,
		StructuredClassInfo @class,
		IReadOnlyList<StructuredMemberInfo> members,
		StructuredNodeInfo? shadows) : base(tree, group)
	{
		Name = name;
		Namespace = @namespace;

		Interface = @interface;
		Class = @class;
		Members = members;

		Shadows = shadows;
		shadows?.ShadowedBy = this;
	}
	#endregion

	#region Methods
	private IEnumerable<StructuredMemberInfo> GetInterfaceMembers()
	{
		IEnumerable<StructuredMemberInfo> members = Members;

		return members.Distinct(StructuredMemberInfo.Comparer.Both);
	}
	private IEnumerable<StructuredMemberInfo> GetClassMembers()
	{
		IEnumerable<StructuredMemberInfo> members = Members;

		if (Group is not null)
			members = members.Concat(Group.Members);

		if (Shadows is not null)
			members = members.Concat(Shadows.ClassMembers.Where(m => HasInterfaceMember(m.Name) is false));

		return members.Distinct(StructuredMemberInfo.Comparer.JustName);
	}
	public bool HasInterfaceMember(Name name) => InterfaceMembers.Any(m => m.Name == name);
	public bool MatchesName(params IEnumerable<Name?> names)
	{
		foreach (Name? current in names.Reverse())
		{
			if (current is null)
				continue;

			if (MatchesName(current.Value))
				return true;
		}

		return false;
	}
	public bool MatchesName(Name name)
	{
		foreach (Name current in Names)
		{
			if (current == name)
				return true;
		}

		return false;
	}
	#endregion
}
